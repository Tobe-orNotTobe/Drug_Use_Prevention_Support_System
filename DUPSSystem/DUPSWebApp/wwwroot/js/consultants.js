// consultants.js - Consultants page functionality
$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata';
    let CURRENT_USER_ID = window.CURRENT_USER_ID || null;

    let allConsultants = [];
    let filteredConsultants = [];
    let userAppointments = [];

    // Check if user is logged in
    if (!CURRENT_USER_ID || !window.USER_TOKEN) {
        showAlert('warning', 'Bạn cần đăng nhập để xem danh sách tư vấn viên');
        setTimeout(() => {
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }, 2000);
        return;
    }

    // Initialize page
    init();

    function init() {
        loadConsultants();
        loadUserAppointmentsStats();
        bindEvents();
    }

    function bindEvents() {
        // Search functionality
        $('#searchBtn').click(function () {
            filterAndDisplayConsultants();
        });

        $('#searchInput').on('input', function () {
            filterAndDisplayConsultants();
        });

        $('#sortFilter').change(function () {
            filterAndDisplayConsultants();
        });

        // Book appointment form
        $('#bookAppointmentForm').submit(function (e) {
            e.preventDefault();
            submitBooking();
        });

        // Modal events
        $('#bookFromDetailBtn').click(function () {
            const consultantId = $(this).data('consultant-id');
            if (consultantId) {
                showBookingModal(consultantId);
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

    // Load all consultants
    function loadConsultants() {
        showLoading();

        const odataQuery = `${API_BASE_URL}/Consultants?$expand=User&$orderby=User/FullName`;

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                allConsultants = response.value || response || [];
                filteredConsultants = [...allConsultants];

                displayConsultants();
                updateStatistics();
                hideLoading();
            },
            error: function (xhr, status, error) {
                hideLoading();
                console.error('Error loading consultants:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 2000);
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi tải danh sách tư vấn viên');
                }
            }
        });
    }

    // Load user appointments for statistics
    function loadUserAppointmentsStats() {
        $.ajax({
            url: `${API_BASE_URL}/Appointments/user/${CURRENT_USER_ID}`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                if (response.success || response.Success) {
                    userAppointments = response.data || response.Data || [];
                    updateStatistics();
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading user appointments:', error);
            }
        });
    }

    // Filter and display consultants
    function filterAndDisplayConsultants() {
        const searchTerm = $('#searchInput').val().toLowerCase().trim();
        const sortFilter = $('#sortFilter').val();

        // Apply search filter
        filteredConsultants = allConsultants.filter(consultant => {
            if (!consultant.User) return false;

            if (searchTerm) {
                const name = consultant.User.FullName?.toLowerCase() || '';
                const expertise = consultant.Expertise?.toLowerCase() || '';
                const qualification = consultant.Qualification?.toLowerCase() || '';

                return name.includes(searchTerm) ||
                    expertise.includes(searchTerm) ||
                    qualification.includes(searchTerm);
            }
            return true;
        });

        // Apply sorting
        switch (sortFilter) {
            case 'name':
                filteredConsultants.sort((a, b) => {
                    const nameA = a.User?.FullName || '';
                    const nameB = b.User?.FullName || '';
                    return nameA.localeCompare(nameB);
                });
                break;
            case 'expertise':
                filteredConsultants.sort((a, b) => {
                    const expertiseA = a.Expertise || '';
                    const expertiseB = b.Expertise || '';
                    return expertiseA.localeCompare(expertiseB);
                });
                break;
            case 'experience':
                filteredConsultants.sort((a, b) => {
                    const appointmentsA = a.Appointments?.length || 0;
                    const appointmentsB = b.Appointments?.length || 0;
                    return appointmentsB - appointmentsA; // Descending order
                });
                break;
        }

        displayConsultants();
    }

    // Display consultants in grid
    function displayConsultants() {
        const consultantsGrid = $('#consultantsGrid');
        const emptyState = $('#emptyState');

        consultantsGrid.empty();

        if (filteredConsultants.length === 0) {
            consultantsGrid.hide();
            emptyState.show();
            return;
        }

        emptyState.hide();
        consultantsGrid.show();

        filteredConsultants.forEach(consultant => {
            if (!consultant.User) return;

            const isActive = consultant.User.IsActive;
            const appointmentCount = consultant.Appointments?.length || 0;

            const consultantCard = `
                <div class="col-lg-4 col-md-6 mb-4">
                    <div class="card h-100 consultant-card" data-consultant-id="${consultant.ConsultantId}">
                        <div class="card-header ${isActive ? 'bg-success' : 'bg-secondary'} text-white">
                            <h5 class="card-title mb-0">
                                <i class="fas fa-user-md"></i> ${escapeHtml(consultant.User.FullName)}
                            </h5>
                            <span class="badge bg-light text-dark float-end">
                                ${isActive ? 'Khả dụng' : 'Không khả dụng'}
                            </span>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                <h6><i class="fas fa-graduation-cap"></i> Chuyên môn:</h6>
                                <p class="text-muted">${consultant.Expertise || 'Chưa cập nhật'}</p>
                            </div>
                            
                            <div class="mb-3">
                                <h6><i class="fas fa-certificate"></i> Bằng cấp:</h6>
                                <p class="text-muted">${consultant.Qualification || 'Chưa cập nhật'}</p>
                            </div>

                            ${consultant.User.Phone ? `
                                <div class="mb-3">
                                    <h6><i class="fas fa-phone"></i> Liên hệ:</h6>
                                    <p class="text-muted">${consultant.User.Phone}</p>
                                </div>
                            ` : ''}

                            <div class="mb-3">
                                <small class="text-muted">
                                    <i class="fas fa-calendar-check"></i> 
                                    ${appointmentCount} lượt tư vấn
                                </small>
                            </div>

                            ${consultant.Bio ? `
                                <div class="mb-3">
                                    <p class="text-muted small">${escapeHtml(consultant.Bio.substring(0, 100))}${consultant.Bio.length > 100 ? '...' : ''}</p>
                                </div>
                            ` : ''}
                        </div>
                        <div class="card-footer">
                            <div class="btn-group w-100" role="group">
                                <button type="button" 
                                        class="btn btn-info btn-sm" 
                                        onclick="viewConsultantDetail(${consultant.ConsultantId})"
                                        title="Xem chi tiết">
                                    <i class="fas fa-eye"></i> Chi tiết
                                </button>
                                ${isActive ? `
                                    <button type="button" 
                                            class="btn btn-primary btn-sm" 
                                            onclick="showBookingModal(${consultant.ConsultantId})"
                                            title="Đặt lịch hẹn">
                                        <i class="fas fa-calendar-plus"></i> Đặt lịch
                                    </button>
                                ` : `
                                    <button type="button" 
                                            class="btn btn-secondary btn-sm disabled"
                                            title="Tư vấn viên không khả dụng">
                                        <i class="fas fa-ban"></i> Không khả dụng
                                    </button>
                                `}
                            </div>
                        </div>
                    </div>
                </div>
            `;

            consultantsGrid.append(consultantCard);
        });
    }

    // Update statistics
    function updateStatistics() {
        const totalConsultants = allConsultants.length;
        const availableConsultants = allConsultants.filter(c => c.User?.IsActive).length;
        const myAppointmentsCount = userAppointments.length;
        const pendingAppointments = userAppointments.filter(a => a.Status === 'Pending').length;

        $('#totalConsultants').text(totalConsultants);
        $('#availableConsultants').text(availableConsultants);
        $('#myAppointments').text(myAppointmentsCount);
        $('#pendingAppointments').text(pendingAppointments);
    }

    // View consultant detail
    function viewConsultantDetail(consultantId) {
        const consultant = allConsultants.find(c => c.ConsultantId === consultantId);
        if (!consultant) return;

        const detailContent = `
            <div class="row">
                <div class="col-md-8">
                    <h4><i class="fas fa-user-md"></i> ${escapeHtml(consultant.User?.FullName || 'Không xác định')}</h4>
                    <p class="text-muted">${consultant.User?.Email || 'Không xác định'}</p>
                </div>
                <div class="col-md-4">
                    <span class="badge ${consultant.User?.IsActive ? 'bg-success' : 'bg-secondary'} fs-6 p-2">
                        ${consultant.User?.IsActive ? 'Khả dụng' : 'Không khả dụng'}
                    </span>
                </div>
            </div>
            
            <hr>
            
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-graduation-cap"></i> Chuyên môn:</h6>
                    <p>${consultant.Expertise || 'Chưa cập nhật'}</p>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-certificate"></i> Bằng cấp:</h6>
                    <p>${consultant.Qualification || 'Chưa cập nhật'}</p>
                </div>
            </div>
            
            ${consultant.User?.Phone ? `
                <div class="row">
                    <div class="col-md-6">
                        <h6><i class="fas fa-phone"></i> Số điện thoại:</h6>
                        <p>${consultant.User.Phone}</p>
                    </div>
                </div>
            ` : ''}

            ${consultant.WorkSchedule ? `
                <div class="row">
                    <div class="col-12">
                        <h6><i class="fas fa-clock"></i> Lịch làm việc:</h6>
                        <p>${escapeHtml(consultant.WorkSchedule)}</p>
                    </div>
                </div>
            ` : ''}
            
            ${consultant.Bio ? `
                <div class="row">
                    <div class="col-12">
                        <h6><i class="fas fa-info-circle"></i> Giới thiệu:</h6>
                        <p>${escapeHtml(consultant.Bio)}</p>
                    </div>
                </div>
            ` : ''}
            
            <div class="row">
                <div class="col-12">
                    <h6><i class="fas fa-chart-bar"></i> Thống kê:</h6>
                    <p>Đã tư vấn: <strong>${consultant.Appointments?.length || 0}</strong> lượt</p>
                </div>
            </div>
        `;

        $('#consultantDetailContent').html(detailContent);
        $('#bookFromDetailBtn').data('consultant-id', consultantId);

        if (consultant.User?.IsActive) {
            $('#bookFromDetailBtn').show();
        } else {
            $('#bookFromDetailBtn').hide();
        }

        new bootstrap.Modal(document.getElementById('consultantDetailModal')).show();
    }

    // Show booking modal
    function showBookingModal(consultantId) {
        const consultant = allConsultants.find(c => c.ConsultantId === consultantId);
        if (!consultant) return;

        // Close detail modal if open
        const detailModal = bootstrap.Modal.getInstance(document.getElementById('consultantDetailModal'));
        if (detailModal) {
            detailModal.hide();
        }

        // Populate consultant info
        const consultantInfo = `
            <div class="d-flex align-items-center">
                <div class="me-3">
                    <i class="fas fa-user-md fa-2x text-primary"></i>
                </div>
                <div>
                    <h6 class="mb-1">${escapeHtml(consultant.User?.FullName || 'Không xác định')}</h6>
                    <small class="text-muted">${consultant.Expertise || 'Tư vấn viên'}</small>
                </div>
            </div>
        `;

        $('#selectedConsultantInfo').html(consultantInfo);
        $('#selectedConsultantId').val(consultantId);

        // Set minimum date to tomorrow
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        $('#appointmentDate').attr('min', tomorrow.toISOString().split('T')[0]);

        // Reset form
        $('#bookAppointmentForm')[0].reset();
        $('#selectedConsultantId').val(consultantId);

        new bootstrap.Modal(document.getElementById('bookAppointmentModal')).show();
    }

    // Submit booking
    function submitBooking() {
        const formData = {
            userId: CURRENT_USER_ID,
            consultantId: parseInt($('#selectedConsultantId').val()),
            appointmentDate: $('#appointmentDate').val() + 'T' + $('#appointmentTime').val() + ':00',
            durationMinutes: parseInt($('#durationMinutes').val()),
            notes: $('#notes').val().trim()
        };

        if (!formData.consultantId || !formData.appointmentDate) {
            showAlert('warning', 'Vui lòng điền đầy đủ thông tin bắt buộc');
            return;
        }

        // Show loading state
        $('#submitBookingBtn').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang đặt lịch...');

        $.ajax({
            url: `${API_BASE_URL}/Appointments`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success || response.Success) {
                    showAlert('success', 'Đặt lịch hẹn thành công!');
                    bootstrap.Modal.getInstance(document.getElementById('bookAppointmentModal')).hide();
                    loadUserAppointmentsStats(); // Refresh stats
                } else {
                    showAlert('error', response.message || response.Message || 'Đã xảy ra lỗi khi đặt lịch hẹn');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error booking appointment:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn');
                } else {
                    const errorMessage = xhr.responseJSON?.message || xhr.responseJSON?.Message || 'Đã xảy ra lỗi khi đặt lịch hẹn';
                    showAlert('error', errorMessage);
                }
            },
            complete: function () {
                $('#submitBookingBtn').prop('disabled', false).html('<i class="fas fa-calendar-plus"></i> Đặt lịch hẹn');
            }
        });
    }

    // Utility functions
    function showLoading() {
        $('#loadingSpinner').show();
        $('#consultantsGrid').hide();
        $('#emptyState').hide();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
        $('#consultantsGrid').show();
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

    // Make functions global for onclick handlers
    window.viewConsultantDetail = viewConsultantDetail;
    window.showBookingModal = showBookingModal;
});