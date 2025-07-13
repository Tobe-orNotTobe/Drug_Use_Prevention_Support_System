// my-appointments.js - My Appointments page functionality
$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata';
    let CURRENT_USER_ID = window.CURRENT_USER_ID || null;

    let userAppointments = [];
    let filteredAppointments = [];

    // Check if user is logged in
    if (!CURRENT_USER_ID || !window.USER_TOKEN) {
        showAlert('warning', 'Bạn cần đăng nhập để xem lịch hẹn của mình');
        setTimeout(() => {
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }, 2000);
        return;
    }

    // Initialize page
    init();

    function init() {
        loadUserAppointments();
        bindEvents();
    }

    function bindEvents() {
        // Filter and search functionality
        $('#statusFilter, #sortFilter').change(function () {
            filterAndDisplayAppointments();
        });

        $('#searchInput').on('input', function () {
            filterAndDisplayAppointments();
        });

        // Cancel appointment
        $('#confirmCancelBtn').click(function () {
            const appointmentId = $(this).data('appointment-id');
            if (appointmentId) {
                cancelAppointment(appointmentId);
            }
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        return {
            'Authorization': `Bearer ${window.USER_TOKEN}`,
            'Content-Type': 'application/json'
        };
    }

    // Load user appointments
    function loadUserAppointments() {
        showLoading();

        $.ajax({
            url: `${API_BASE_URL}/Appointments/user/${CURRENT_USER_ID}`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                hideLoading();

                if (response.success || response.Success) {
                    userAppointments = response.data || response.Data || [];
                    filteredAppointments = [...userAppointments];

                    displayAppointments();
                    updateStatistics();
                } else {
                    showAlert('error', response.message || 'Đã xảy ra lỗi khi tải lịch hẹn');
                }
            },
            error: function (xhr, status, error) {
                hideLoading();
                console.error('Error loading appointments:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 2000);
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi tải lịch hẹn');
                }
            }
        });
    }

    // Filter and display appointments
    function filterAndDisplayAppointments() {
        const statusFilter = $('#statusFilter').val();
        const sortFilter = $('#sortFilter').val();
        const searchTerm = $('#searchInput').val().toLowerCase().trim();

        // Apply filters
        filteredAppointments = userAppointments.filter(appointment => {
            // Status filter
            if (statusFilter && appointment.Status !== statusFilter) {
                return false;
            }

            // Search filter
            if (searchTerm) {
                const consultantName = appointment.ConsultantName?.toLowerCase() || '';
                const notes = appointment.Notes?.toLowerCase() || '';

                if (!consultantName.includes(searchTerm) && !notes.includes(searchTerm)) {
                    return false;
                }
            }

            return true;
        });

        // Apply sorting
        switch (sortFilter) {
            case 'newest':
                filteredAppointments.sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt));
                break;
            case 'oldest':
                filteredAppointments.sort((a, b) => new Date(a.CreatedAt) - new Date(b.CreatedAt));
                break;
            case 'date_asc':
                filteredAppointments.sort((a, b) => new Date(a.AppointmentDate) - new Date(b.AppointmentDate));
                break;
            case 'date_desc':
                filteredAppointments.sort((a, b) => new Date(b.AppointmentDate) - new Date(a.AppointmentDate));
                break;
        }

        displayAppointments();
    }

    // Display appointments
    function displayAppointments() {
        const appointmentsList = $('#appointmentsList');
        const emptyState = $('#emptyState');

        appointmentsList.empty();

        if (filteredAppointments.length === 0) {
            appointmentsList.hide();
            emptyState.show();
            return;
        }

        emptyState.hide();
        appointmentsList.show();

        filteredAppointments.forEach(appointment => {
            const appointmentDate = new Date(appointment.AppointmentDate);
            const isUpcoming = appointmentDate > new Date();
            const canCancel = appointment.Status === 'Pending' || appointment.Status === 'Confirmed';

            const statusColor = getStatusColor(appointment.Status);
            const statusIcon = getStatusIcon(appointment.Status);

            const appointmentCard = `
                <div class="card mb-3 appointment-card" data-appointment-id="${appointment.AppointmentId}">
                    <div class="card-header bg-${statusColor} text-white">
                        <div class="row align-items-center">
                            <div class="col-md-8">
                                <h6 class="mb-0">
                                    <i class="${statusIcon}"></i> 
                                    Tư vấn với ${escapeHtml(appointment.ConsultantName)}
                                </h6>
                                <small>Chuyên môn: ${escapeHtml(appointment.ConsultantExpertise)}</small>
                            </div>
                            <div class="col-md-4 text-end">
                                <span class="badge bg-light text-dark">
                                    ${getStatusText(appointment.Status)}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="mb-2">
                                    <i class="fas fa-calendar text-primary"></i>
                                    <strong>Ngày hẹn:</strong> ${formatDateTime(appointment.AppointmentDate)}
                                </div>
                                <div class="mb-2">
                                    <i class="fas fa-clock text-primary"></i>
                                    <strong>Thời lượng:</strong> ${appointment.DurationMinutes || 60} phút
                                </div>
                            </div>
                            <div class="col-md-6">
                                <div class="mb-2">
                                    <i class="fas fa-calendar-plus text-primary"></i>
                                    <strong>Ngày đặt:</strong> ${formatDate(appointment.CreatedAt)}
                                </div>
                                ${isUpcoming ? `
                                    <div class="mb-2">
                                        <i class="fas fa-hourglass-half text-warning"></i>
                                        <strong>Còn lại:</strong> ${getTimeUntilAppointment(appointment.AppointmentDate)}
                                    </div>
                                ` : ''}
                            </div>
                        </div>

                        ${appointment.Notes ? `
                            <div class="row mt-3">
                                <div class="col-12">
                                    <div class="alert alert-light">
                                        <i class="fas fa-sticky-note"></i>
                                        <strong>Ghi chú:</strong> ${escapeHtml(appointment.Notes)}
                                    </div>
                                </div>
                            </div>
                        ` : ''}

                        <div class="mt-3">
                            <div class="btn-group" role="group">
                                <button type="button" 
                                        class="btn btn-info btn-sm" 
                                        onclick="viewAppointmentDetail(${appointment.AppointmentId})"
                                        title="Xem chi tiết">
                                    <i class="fas fa-eye"></i> Chi tiết
                                </button>
                                ${canCancel ? `
                                    <button type="button" 
                                            class="btn btn-danger btn-sm" 
                                            onclick="showCancelConfirmation(${appointment.AppointmentId})"
                                            title="Hủy lịch hẹn">
                                        <i class="fas fa-times"></i> Hủy
                                    </button>
                                ` : ''}
                                ${appointment.Status === 'Confirmed' && isUpcoming ? `
                                    <button type="button" 
                                            class="btn btn-success btn-sm" 
                                            title="Sẵn sàng tham gia">
                                        <i class="fas fa-check"></i> Sẵn sàng
                                    </button>
                                ` : ''}
                            </div>
                        </div>
                    </div>
                </div>
            `;

            appointmentsList.append(appointmentCard);
        });
    }

    // Update statistics
    function updateStatistics() {
        const total = userAppointments.length;
        const pending = userAppointments.filter(a => a.Status === 'Pending').length;
        const confirmed = userAppointments.filter(a => a.Status === 'Confirmed').length;
        const completed = userAppointments.filter(a => a.Status === 'Completed').length;

        $('#totalAppointments').text(total);
        $('#pendingCount').text(pending);
        $('#confirmedCount').text(confirmed);
        $('#completedCount').text(completed);
    }

    // View appointment detail
    function viewAppointmentDetail(appointmentId) {
        const appointment = userAppointments.find(a => a.AppointmentId === appointmentId);
        if (!appointment) return;

        const isUpcoming = new Date(appointment.AppointmentDate) > new Date();
        const canCancel = appointment.Status === 'Pending' || appointment.Status === 'Confirmed';

        const detailContent = `
            <div class="row">
                <div class="col-md-8">
                    <h4><i class="fas fa-calendar-check"></i> Chi tiết lịch hẹn</h4>
                    <p class="text-muted">Mã lịch hẹn: #${appointment.AppointmentId}</p>
                </div>
                <div class="col-md-4">
                    <span class="badge bg-${getStatusColor(appointment.Status)} fs-6 p-2">
                        ${getStatusText(appointment.Status)}
                    </span>
                </div>
            </div>
            
            <hr>
            
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-user-md"></i> Tư vấn viên:</h6>
                    <p>${escapeHtml(appointment.ConsultantName)}</p>
                    <small class="text-muted">${escapeHtml(appointment.ConsultantExpertise)}</small>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-calendar"></i> Ngày và giờ hẹn:</h6>
                    <p><strong>${formatDateTime(appointment.AppointmentDate)}</strong></p>
                </div>
            </div>
            
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-clock"></i> Thời lượng:</h6>
                    <p>${appointment.DurationMinutes || 60} phút</p>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-calendar-plus"></i> Ngày đặt:</h6>
                    <p>${formatDateTime(appointment.CreatedAt)}</p>
                </div>
            </div>
            
            ${appointment.Notes ? `
                <div class="row">
                    <div class="col-12">
                        <h6><i class="fas fa-sticky-note"></i> Ghi chú:</h6>
                        <div class="alert alert-light">
                            ${escapeHtml(appointment.Notes)}
                        </div>
                    </div>
                </div>
            ` : ''}

            ${isUpcoming ? `
                <div class="alert alert-info">
                    <i class="fas fa-info-circle"></i>
                    <strong>Lưu ý:</strong> Còn ${getTimeUntilAppointment(appointment.AppointmentDate)} nữa đến lịch hẹn của bạn.
                </div>
            ` : ''}

            ${appointment.Status === 'Completed' ? `
                <div class="alert alert-success">
                    <i class="fas fa-check-circle"></i>
                    <strong>Hoàn thành:</strong> Buổi tư vấn đã kết thúc thành công.
                </div>
            ` : ''}
        `;

        $('#appointmentDetailContent').html(detailContent);

        // Setup modal buttons
        if (canCancel) {
            $('#cancelAppointmentBtn').show().data('appointment-id', appointmentId);
        } else {
            $('#cancelAppointmentBtn').hide();
        }

        new bootstrap.Modal(document.getElementById('appointmentDetailModal')).show();
    }

    // Show cancel confirmation
    function showCancelConfirmation(appointmentId) {
        $('#confirmCancelBtn').data('appointment-id', appointmentId);
        new bootstrap.Modal(document.getElementById('cancelConfirmModal')).show();
    }

    // Cancel appointment
    function cancelAppointment(appointmentId) {
        const cancelData = {
            userId: CURRENT_USER_ID
        };

        $.ajax({
            url: `${API_BASE_URL}/Appointments(${appointmentId})/cancel`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(cancelData),
            success: function (response) {
                if (response.success || response.Success) {
                    showAlert('success', 'Hủy lịch hẹn thành công');

                    // Hide modals
                    bootstrap.Modal.getInstance(document.getElementById('cancelConfirmModal')).hide();
                    const detailModal = bootstrap.Modal.getInstance(document.getElementById('appointmentDetailModal'));
                    if (detailModal) {
                        detailModal.hide();
                    }

                    // Reload appointments
                    loadUserAppointments();
                } else {
                    showAlert('error', response.message || response.Message || 'Đã xảy ra lỗi khi hủy lịch hẹn');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error cancelling appointment:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn');
                } else {
                    const errorMessage = xhr.responseJSON?.message || xhr.responseJSON?.Message || 'Đã xảy ra lỗi khi hủy lịch hẹn';
                    showAlert('error', errorMessage);
                }
            }
        });
    }

    // Utility functions
    function getStatusColor(status) {
        switch (status) {
            case 'Pending': return 'warning';
            case 'Confirmed': return 'info';
            case 'Completed': return 'success';
            case 'Cancelled': return 'secondary';
            default: return 'light';
        }
    }

    function getStatusIcon(status) {
        switch (status) {
            case 'Pending': return 'fas fa-clock';
            case 'Confirmed': return 'fas fa-check';
            case 'Completed': return 'fas fa-check-double';
            case 'Cancelled': return 'fas fa-times';
            default: return 'fas fa-question';
        }
    }

    function getStatusText(status) {
        switch (status) {
            case 'Pending': return 'Chờ xác nhận';
            case 'Confirmed': return 'Đã xác nhận';
            case 'Completed': return 'Đã hoàn thành';
            case 'Cancelled': return 'Đã hủy';
            default: return status || 'Không xác định';
        }
    }

    function getTimeUntilAppointment(appointmentDate) {
        const now = new Date();
        const appointment = new Date(appointmentDate);
        const diff = appointment - now;

        if (diff <= 0) return 'Đã qua';

        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));

        if (days > 0) {
            return `${days} ngày ${hours} giờ`;
        } else if (hours > 0) {
            return `${hours} giờ ${minutes} phút`;
        } else {
            return `${minutes} phút`;
        }
    }

    function showLoading() {
        $('#loadingSpinner').show();
        $('#appointmentsList').hide();
        $('#emptyState').hide();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
        $('#appointmentsList').show();
    }

    function showAlert(type, message) {
        const alertClass = {
            'success': 'alert-success',
            'error': 'alert-danger',
            'warning': 'alert-warning',
            'info': 'alert-info'
        }[type] || 'alert-info';

        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 1050; min-width: 300px;" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        // Remove existing alerts
        $('.alert.position-fixed').remove();

        // Add new alert
        $('body').append(alertHtml);

        // Auto remove after 5 seconds
        setTimeout(() => {
            $('.alert.position-fixed').fadeOut();
        }, 5000);
    }

    function escapeHtml(text) {
        if (!text) return '';
        return text
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    function formatDateTime(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Make functions global for onclick handlers
    window.viewAppointmentDetail = viewAppointmentDetail;
    window.showCancelConfirmation = showCancelConfirmation;
});