$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata';
    const CURRENT_USER_ID = window.CURRENT_USER_ID || null;
    const USER_PERMISSIONS = window.USER_PERMISSIONS || {};

    let currentPage = 1;
    let pageSize = 10;
    let totalRecords = 0;

    // Initialize page
    init();

    function init() {
        loadConsultants(); // Load consultants for filter
        loadAppointments();
        if (USER_PERMISSIONS.canViewAllAppointments) {
            loadAppointmentStatistics();
        }
        bindEvents();
    }

    function bindEvents() {
        // Filter functionality
        $('#filterBtn').click(function () {
            currentPage = 1;
            loadAppointments();
        });

        $('#statusFilter, #dateFilter, #consultantFilter').change(function () {
            currentPage = 1;
            loadAppointments();
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        const headers = {
            'Content-Type': 'application/json'
        };

        if (USER_PERMISSIONS.isAuthenticated && window.USER_TOKEN) {
            headers['Authorization'] = `Bearer ${window.USER_TOKEN}`;
        }

        return headers;
    }

    // Load consultants for filter dropdown
    function loadConsultants() {
        $.ajax({
            url: `${API_BASE_URL}/Consultants?$select=ConsultantId,FullName&$filter=IsActive eq true`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const consultants = response.value || response;
                const select = $('#consultantFilter');

                consultants.forEach(consultant => {
                    select.append(`<option value="${consultant.ConsultantId}">${consultant.FullName}</option>`);
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading consultants:', error);
            }
        });
    }

    // Load appointments based on user role
    function loadAppointments() {
        showLoading();

        let odataQuery = `${API_BASE_URL}/Appointments?`;
        let filters = [];

        // Role-based filtering
        if (USER_PERMISSIONS.isConsultant && !USER_PERMISSIONS.canViewAllAppointments) {
            // Consultants can only see their own appointments
            filters.push(`ConsultantId eq ${CURRENT_USER_ID}`);
        } else if (!USER_PERMISSIONS.canViewAllAppointments) {
            // Regular users can only see their own appointments
            filters.push(`UserId eq ${CURRENT_USER_ID}`);
        }

        // Status filter
        const status = $('#statusFilter').val();
        if (status) {
            filters.push(`Status eq '${status}'`);
        }

        // Date filter
        const dateFilter = $('#dateFilter').val();
        if (dateFilter) {
            filters.push(`date(AppointmentDate) eq ${dateFilter}`);
        }

        // Consultant filter
        const consultantId = $('#consultantFilter').val();
        if (consultantId) {
            filters.push(`ConsultantId eq ${consultantId}`);
        }

        // Apply filters
        if (filters.length > 0) {
            odataQuery += `$filter=${filters.join(' and ')}&`;
        }

        // Add expansion, ordering and pagination
        odataQuery += `$expand=User,Consultant&$orderby=AppointmentDate desc&$top=${pageSize}&$skip=${(currentPage - 1) * pageSize}&$count=true`;

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                hideLoading();
                const appointments = response.value || response;
                displayAppointments(appointments);
                totalRecords = response['@odata.count'] || appointments.length;
                updatePagination();
            },
            error: function (xhr, status, error) {
                hideLoading();
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi tải danh sách lịch hẹn');
            }
        });
    }

    // Display appointments with role-based action buttons
    function displayAppointments(appointments) {
        const tbody = $('#appointmentsTableBody');
        tbody.empty();

        if (!appointments || appointments.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="7" class="text-center">Không có lịch hẹn nào</td>
                </tr>
            `);
            return;
        }

        appointments.forEach(appointment => {
            const actionButtons = generateAppointmentActionButtons(appointment);

            const row = `
                <tr>
                    <td>${appointment.AppointmentId}</td>
                    <td>
                        <strong>${escapeHtml(appointment.Consultant?.FullName || 'N/A')}</strong>
                        ${appointment.Consultant?.Specialization ? `<br><small class="text-muted">${escapeHtml(appointment.Consultant.Specialization)}</small>` : ''}
                    </td>
                    <td>${formatDate(appointment.AppointmentDate)}</td>
                    <td>${formatTime(appointment.AppointmentTime)}</td>
                    <td>
                        <span class="badge bg-${getStatusClass(appointment.Status)}">
                            ${getStatusText(appointment.Status)}
                        </span>
                    </td>
                    <td>
                        ${appointment.Notes ? `<small>${escapeHtml(appointment.Notes.substring(0, 50))}${appointment.Notes.length > 50 ? '...' : ''}</small>` : '-'}
                    </td>
                    <td>${actionButtons}</td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Generate action buttons based on user permissions and appointment status
    function generateAppointmentActionButtons(appointment) {
        let buttons = '';

        // Everyone can view details
        buttons += `<button type="button" class="btn btn-info btn-sm me-1" onclick="viewAppointment(${appointment.AppointmentId})" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button>`;

        // User actions
        if (appointment.UserId === CURRENT_USER_ID || USER_PERMISSIONS.canManageAppointments) {
            // Can edit if pending and user owns it or has management rights
            if (appointment.Status === 'Pending') {
                buttons += `<button type="button" class="btn btn-warning btn-sm me-1" onclick="editAppointment(${appointment.AppointmentId})" title="Chỉnh sửa">
                                <i class="fas fa-edit"></i>
                            </button>`;
            }

            // Can cancel if not completed/cancelled
            if (appointment.Status !== 'Completed' && appointment.Status !== 'Cancelled') {
                buttons += `<button type="button" class="btn btn-danger btn-sm me-1" onclick="cancelAppointment(${appointment.AppointmentId})" title="Hủy lịch hẹn">
                                <i class="fas fa-times"></i>
                            </button>`;
            }
        }

        // Consultant actions
        if (USER_PERMISSIONS.isConsultant && appointment.ConsultantId === CURRENT_USER_ID) {
            // Confirm appointment
            if (appointment.Status === 'Pending') {
                buttons += `<button type="button" class="btn btn-success btn-sm me-1" onclick="confirmAppointment(${appointment.AppointmentId})" title="Xác nhận">
                                <i class="fas fa-check"></i>
                            </button>`;
            }

            // Complete appointment and add notes
            if (appointment.Status === 'Confirmed') {
                buttons += `<button type="button" class="btn btn-primary btn-sm me-1" onclick="completeAppointment(${appointment.AppointmentId})" title="Hoàn thành">
                                <i class="fas fa-clipboard-check"></i>
                            </button>`;
            }

            // View/Add consultation notes
            buttons += `<button type="button" class="btn btn-secondary btn-sm" onclick="viewConsultationNotes(${appointment.AppointmentId})" title="Ghi chú tư vấn">
                            <i class="fas fa-sticky-note"></i>
                        </button>`;
        }

        // Staff+ management actions
        if (USER_PERMISSIONS.canManageAppointments) {
            buttons += `<button type="button" class="btn btn-dark btn-sm" onclick="manageAppointment(${appointment.AppointmentId})" title="Quản lý">
                            <i class="fas fa-cog"></i>
                        </button>`;
        }

        return `<div class="btn-group btn-group-sm" role="group">${buttons}</div>`;
    }

    // Load appointment statistics (for management roles)
    function loadAppointmentStatistics() {
        // Total appointments
        $.ajax({
            url: `${API_BASE_URL}/Appointments?$count=true`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const total = response['@odata.count'] || 0;
                $('#totalAppointments').text(total);
            }
        });

        // Pending appointments
        $.ajax({
            url: `${API_BASE_URL}/Appointments?$filter=Status eq 'Pending'&$count=true`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const pending = response['@odata.count'] || 0;
                $('#pendingAppointments').text(pending);
            }
        });

        // Completed appointments
        $.ajax({
            url: `${API_BASE_URL}/Appointments?$filter=Status eq 'Completed'&$count=true`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const completed = response['@odata.count'] || 0;
                $('#completedAppointments').text(completed);
            }
        });

        // Cancelled appointments
        $.ajax({
            url: `${API_BASE_URL}/Appointments?$filter=Status eq 'Cancelled'&$count=true`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const cancelled = response['@odata.count'] || 0;
                $('#cancelledAppointments').text(cancelled);
            }
        });
    }

    // Appointment action functions
    window.viewAppointment = function (appointmentId) {
        window.location.href = `/Appointments/Details/${appointmentId}`;
    };

    window.editAppointment = function (appointmentId) {
        window.location.href = `/Appointments/Edit/${appointmentId}`;
    };

    window.cancelAppointment = function (appointmentId) {
        if (confirm('Bạn có chắc chắn muốn hủy lịch hẹn này?')) {
            updateAppointmentStatus(appointmentId, 'Cancelled');
        }
    };

    window.confirmAppointment = function (appointmentId) {
        if (!USER_PERMISSIONS.isConsultant) {
            showAlert('error', 'Chỉ tư vấn viên mới có thể xác nhận lịch hẹn');
            return;
        }
        updateAppointmentStatus(appointmentId, 'Confirmed');
    };

    window.completeAppointment = function (appointmentId) {
        if (!USER_PERMISSIONS.isConsultant) {
            showAlert('error', 'Chỉ tư vấn viên mới có thể hoàn thành lịch hẹn');
            return;
        }

        // Show modal for completion notes
        showCompletionModal(appointmentId);
    };

    window.viewConsultationNotes = function (appointmentId) {
        window.location.href = `/Appointments/ConsultationNotes/${appointmentId}`;
    };

    window.manageAppointment = function (appointmentId) {
        if (!USER_PERMISSIONS.canManageAppointments) {
            showAlert('error', 'Bạn không có quyền quản lý lịch hẹn');
            return;
        }
        window.location.href = `/Appointments/Manage/${appointmentId}`;
    };

    // Update appointment status
    function updateAppointmentStatus(appointmentId, status) {
        const data = {
            Status: status,
            UpdatedAt: new Date().toISOString()
        };

        $.ajax({
            url: `${API_BASE_URL}/Appointments(${appointmentId})`,
            method: 'PATCH',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(data),
            success: function (response) {
                showAlert('success', `Cập nhật trạng thái lịch hẹn thành công!`);
                loadAppointments(); // Reload appointments
                if (USER_PERMISSIONS.canViewAllAppointments) {
                    loadAppointmentStatistics(); // Reload statistics
                }
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi cập nhật trạng thái lịch hẹn');
            }
        });
    }

    // Show completion modal
    function showCompletionModal(appointmentId) {
        const modalHtml = `
            <div class="modal fade" id="completionModal" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Hoàn thành lịch hẹn</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label for="consultationNotes" class="form-label">Ghi chú tư vấn <span class="text-danger">*</span></label>
                                <textarea class="form-control" id="consultationNotes" rows="4" placeholder="Nhập ghi chú về buổi tư vấn..."></textarea>
                            </div>
                            <div class="mb-3">
                                <label for="recommendations" class="form-label">Khuyến nghị</label>
                                <textarea class="form-control" id="recommendations" rows="3" placeholder="Khuyến nghị cho khách hàng..."></textarea>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                            <button type="button" class="btn btn-primary" onclick="completeAppointmentWithNotes(${appointmentId})">Hoàn thành</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if any
        $('#completionModal').remove();

        // Add modal to body and show
        $('body').append(modalHtml);
        $('#completionModal').modal('show');
    }

    // Complete appointment with notes
    window.completeAppointmentWithNotes = function (appointmentId) {
        const consultationNotes = $('#consultationNotes').val().trim();
        const recommendations = $('#recommendations').val().trim();

        if (!consultationNotes) {
            showAlert('error', 'Vui lòng nhập ghi chú tư vấn');
            return;
        }

        const data = {
            Status: 'Completed',
            ConsultationNotes: consultationNotes,
            Recommendations: recommendations,
            CompletedAt: new Date().toISOString(),
            UpdatedAt: new Date().toISOString()
        };

        $.ajax({
            url: `${API_BASE_URL}/Appointments(${appointmentId})`,
            method: 'PATCH',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(data),
            success: function (response) {
                $('#completionModal').modal('hide');
                showAlert('success', 'Hoàn thành lịch hẹn thành công!');
                loadAppointments(); // Reload appointments
                if (USER_PERMISSIONS.canViewAllAppointments) {
                    loadAppointmentStatistics(); // Reload statistics
                }
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi hoàn thành lịch hẹn');
            }
        });
    };

    // Helper functions
    function getStatusClass(status) {
        const classes = {
            'Pending': 'warning',
            'Confirmed': 'info',
            'Completed': 'success',
            'Cancelled': 'danger'
        };
        return classes[status] || 'secondary';
    }

    function getStatusText(status) {
        const texts = {
            'Pending': 'Chờ xác nhận',
            'Confirmed': 'Đã xác nhận',
            'Completed': 'Hoàn thành',
            'Cancelled': 'Đã hủy'
        };
        return texts[status] || status;
    }

    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    function formatTime(timeString) {
        if (!timeString) return 'N/A';
        return timeString.substring(0, 5); // Format HH:MM
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function handleAjaxError(xhr, defaultMessage) {
        if (xhr.status === 401) {
            showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
            setTimeout(() => {
                window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            }, 2000);
        } else if (xhr.status === 403) {
            showAlert('error', 'Bạn không có quyền thực hiện hành động này.');
        } else {
            showAlert('error', defaultMessage);
        }
    }

    function showLoading() {
        $('#loadingSpinner').show();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
    }

    function showAlert(type, message) {
        const alertClass = type === 'success' ? 'alert-success' : type === 'error' ? 'alert-danger' : 'alert-warning';
        const alertHtml = `<div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                              ${message}
                              <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                           </div>`;
        $('.container-fluid').prepend(alertHtml);

        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }

    function updatePagination() {
        const totalPages = Math.ceil(totalRecords / pageSize);
        const pagination = $('#pagination');
        pagination.empty();

        for (let i = 1; i <= totalPages; i++) {
            const activeClass = i === currentPage ? 'active' : '';
            pagination.append(`
                <li class="page-item ${activeClass}">
                    <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
                </li>
            `);
        }
    }

    window.changePage = function (page) {
        currentPage = page;
        loadAppointments();
    };
});