const AppointmentAPI = {
    baseUrl: 'https://localhost:7008',

    getToken() {
        return document.querySelector('meta[name="auth-token"]')?.getAttribute('content') || '';
    },

    getCurrentUserId() {
        return document.querySelector('meta[name="current-user-id"]')?.getAttribute('content') || '';
    },

    getUserRole() {
        return document.body.getAttribute('data-user-role') || 'Guest';
    },

    async request(url, options = {}) {
        const token = this.getToken();
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            }
        };

        const config = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        try {
            const response = await fetch(`${this.baseUrl}${url}`, config);

            // Handle different response types from OData
            if (response.status === 204) {
                // No Content - successful PATCH/PUT/DELETE
                return { success: true };
            }

            if (!response.ok) {
                const errorText = await response.text();
                let errorMessage = `HTTP ${response.status}: ${response.statusText}`;

                try {
                    const errorJson = JSON.parse(errorText);
                    errorMessage = errorJson.message || errorJson.error?.message || errorMessage;
                } catch {
                    errorMessage = errorText || errorMessage;
                }

                throw new Error(errorMessage);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }

            return { success: true };
        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    },

    async getConsultants() {
        return this.request('/odata/Consultants?$expand=User');
    },

    async getMyAppointments() {
        const userId = this.getCurrentUserId();
        const role = this.getUserRole();

        if (role === 'Member') {
            return this.request(`/odata/Appointments?$filter=UserId eq ${userId}&$expand=Consultant($expand=User)`);
        } else if (role === 'Consultant') {
            // Để API tự động filter dựa trên UserId -> ConsultantId mapping
            return this.request('/odata/Appointments?$expand=User,Consultant($expand=User)');
        } else {
            // Staff/Manager/Admin xem tất cả
            return this.request('/odata/Appointments?$expand=User,Consultant($expand=User)');
        }
    },

    async createAppointment(appointmentData) {
        return this.request('/odata/Appointments', {
            method: 'POST',
            body: JSON.stringify(appointmentData)
        });
    },

    async updateAppointment(appointmentId, updateData) {
        return this.request(`/odata/Appointments(${appointmentId})`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.getToken()}`
            },
            body: JSON.stringify(updateData)
        });
    }
};

const AppointmentUtils = {
    showToast(message, type = 'success') {
        const toastContainer = this.getToastContainer();
        const toastId = 'toast-' + Date.now();

        const toastElement = document.createElement('div');
        toastElement.id = toastId;
        toastElement.className = `toast align-items-center text-white bg-${type === 'success' ? 'success' : 'danger'} border-0`;
        toastElement.setAttribute('role', 'alert');
        toastElement.setAttribute('aria-live', 'assertive');
        toastElement.setAttribute('aria-atomic', 'true');

        toastElement.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        toastContainer.appendChild(toastElement);

        const toast = new bootstrap.Toast(toastElement);
        toast.show();

        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    },

    getToastContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }
        return container;
    },

    formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    },

    getStatusBadge(status) {
        const statusMap = {
            'Pending': { class: 'bg-warning text-dark', text: 'Chờ xác nhận', icon: 'clock' },
            'Confirmed': { class: 'bg-primary', text: 'Đã xác nhận', icon: 'check-circle' },
            'Completed': { class: 'bg-success', text: 'Hoàn thành', icon: 'check-double' },
            'Cancelled': { class: 'bg-danger', text: 'Đã hủy', icon: 'times-circle' }
        };

        const statusInfo = statusMap[status] || { class: 'bg-secondary', text: status, icon: 'question' };
        return `<span class="badge ${statusInfo.class}">
                    <i class="fas fa-${statusInfo.icon} me-1"></i>
                    ${statusInfo.text}
                </span>`;
    },

    setMinDate() {
        const today = new Date();
        const tomorrow = new Date(today);
        tomorrow.setDate(tomorrow.getDate() + 1);

        const dateInput = document.getElementById('appointmentDate');
        if (dateInput) {
            dateInput.min = tomorrow.toISOString().split('T')[0];
        }
    }
};

const ConsultantList = {
    consultants: [],

    async init() {
        AppointmentUtils.setMinDate();
        await this.loadConsultants();
        this.bindEvents();
    },

    async loadConsultants() {
        const loadingSpinner = document.getElementById('loading-spinner');
        const consultantsContainer = document.getElementById('consultants-container');
        const emptyMessage = document.getElementById('empty-message');

        try {
            loadingSpinner.classList.remove('d-none');
            consultantsContainer.innerHTML = '';

            const response = await AppointmentAPI.getConsultants();
            this.consultants = response.value || [];

            if (this.consultants.length === 0) {
                emptyMessage.classList.remove('d-none');
            } else {
                this.renderConsultants();
            }
        } catch (error) {
            console.error('Error loading consultants:', error);
            AppointmentUtils.showToast('Không thể tải danh sách tư vấn viên. Vui lòng thử lại.', 'error');
        } finally {
            loadingSpinner.classList.add('d-none');
        }
    },

    renderConsultants() {
        const container = document.getElementById('consultants-container');

        this.consultants.forEach(consultant => {
            const consultantCard = this.createConsultantCard(consultant);
            container.appendChild(consultantCard);
        });
    },

    createConsultantCard(consultant) {
        const col = document.createElement('div');
        col.className = 'col-lg-4 col-md-6';

        col.innerHTML = `
            <div class="card h-100 shadow-sm consultant-card">
                <div class="card-body text-center">
                    <h5 class="card-title mb-2">${consultant.User?.FullName || 'Không xác định'}</h5>
                    <p class="text-muted mb-2">
                        <i class="fas fa-graduation-cap"></i>
                        ${consultant.Expertise || 'Chưa cập nhật'}
                    </p>
                    <p class="text-muted mb-2 small">
                        <i class="fas fa-envelope"></i>
                        ${consultant.User?.Email || 'Không xác định'}
                    </p>
                    <p class="text-muted mb-3 small">
                        <i class="fas fa-phone"></i>
                        ${consultant.User?.Phone || 'Chưa cập nhật'}
                    </p>
                    <button class="btn btn-primary btn-book-appointment" 
                            data-consultant-id="${consultant.ConsultantId}"
                            data-consultant-name="${consultant.User?.FullName || 'Không xác định'}"
                            data-consultant-email="${consultant.User?.Email || 'Không xác định'}"
                            data-consultant-expertise="${consultant.Expertise || 'Chưa cập nhật'}"
                        <i class="fas fa-calendar-plus"></i>
                        Đặt lịch hẹn
                    </button>
                </div>
            </div>
        `;

        return col;
    },

    bindEvents() {
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-book-appointment')) {
                const btn = e.target.closest('.btn-book-appointment');
                this.openAppointmentModal(btn);
            }
        });

        document.getElementById('submitAppointment')?.addEventListener('click', () => {
            this.submitAppointment();
        });

        const modal = document.getElementById('appointmentModal');
        modal?.addEventListener('hidden.bs.modal', () => {
            this.resetForm();
        });
    },

    openAppointmentModal(button) {
        const consultantId = button.dataset.consultantId;
        const consultantName = button.dataset.consultantName;
        const consultantEmail = button.dataset.consultantEmail;
        const consultantExpertise = button.dataset.consultantExpertise;

        document.getElementById('selectedConsultantId').value = consultantId;
        document.getElementById('consultantName').textContent = consultantName;
        document.getElementById('consultantEmail').textContent = consultantEmail;
        document.getElementById('consultantExpertise').textContent = consultantExpertise;

        const modal = new bootstrap.Modal(document.getElementById('appointmentModal'));
        modal.show();
    },

    async submitAppointment() {
        const form = document.getElementById('appointmentForm');
        const submitBtn = document.getElementById('submitAppointment');

        if (!form.checkValidity()) {
            form.classList.add('was-validated');
            return;
        }

        const formData = new FormData(form);

        // Get duration directly from select value (in minutes)
        const durationMinutes = parseInt(formData.get('timeSlot')) || 60;

        const appointmentData = {
            ConsultantId: parseInt(formData.get('consultantId')),
            AppointmentDate: formData.get('date') + 'T00:00:00',
            DurationMinutes: durationMinutes,
            Notes: formData.get('notes') || ''
        };

        try {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

            const response = await AppointmentAPI.createAppointment(appointmentData);

            // OData Created response trả về appointment entity
            if (response && response.AppointmentId) {
                AppointmentUtils.showToast('Đặt lịch hẹn thành công! Tư vấn viên sẽ xác nhận sớm nhất.', 'success');

                const modal = bootstrap.Modal.getInstance(document.getElementById('appointmentModal'));
                modal.hide();
            } else {
                throw new Error('Không thể đặt lịch hẹn');
            }

        } catch (error) {
            console.error('Error creating appointment:', error);
            const errorMessage = error.message || 'Không thể đặt lịch hẹn. Vui lòng thử lại.';
            AppointmentUtils.showToast(errorMessage, 'error');
        } finally {
            submitBtn.disabled = false;
            submitBtn.innerHTML = '<i class="fas fa-calendar-check"></i> Đặt lịch hẹn';
        }
    },

    convertTimeSlotToDuration(timeSlot) {
        // Convert time slot like "09:00-10:00" to duration in minutes
        if (!timeSlot || typeof timeSlot !== 'string') return 60;

        const parts = timeSlot.split('-');
        if (parts.length !== 2) return 60;

        const [startTime, endTime] = parts;
        const [startHour, startMin] = startTime.split(':').map(Number);
        const [endHour, endMin] = endTime.split(':').map(Number);

        const startMinutes = startHour * 60 + startMin;
        const endMinutes = endHour * 60 + endMin;

        return endMinutes - startMinutes;
    },

    convertDurationToTimeSlot(appointmentDate, durationMinutes) {
        // For display purposes, generate a time slot from duration
        // This is a simple implementation - you might want to store actual start time
        const duration = durationMinutes || 60;
        const startHour = 9; // Default start at 9 AM
        const endHour = startHour + Math.floor(duration / 60);
        const endMin = duration % 60;

        return `${startHour.toString().padStart(2, '0')}:00-${endHour.toString().padStart(2, '0')}:${endMin.toString().padStart(2, '0')}`;
    },

    resetForm() {
        const form = document.getElementById('appointmentForm');
        form.reset();
        form.classList.remove('was-validated');
    }
};

const MyAppointment = {
    appointments: [],
    currentRole: 'Guest',
    currentFilter: '',

    async init(role) {
        this.currentRole = role;
        await this.loadAppointments();
        this.bindEvents();
    },

    async loadAppointments() {
        const loadingSpinner = document.getElementById('loading-spinner');
        const tableBody = document.getElementById('appointmentsTableBody');
        const emptyMessage = document.getElementById('empty-message');

        try {
            loadingSpinner.classList.remove('d-none');
            tableBody.innerHTML = '';

            const response = await AppointmentAPI.getMyAppointments();
            this.appointments = response.value || [];

            if (this.appointments.length === 0) {
                emptyMessage.classList.remove('d-none');
                document.getElementById('appointmentsTable').classList.add('d-none');
            } else {
                emptyMessage.classList.add('d-none');
                document.getElementById('appointmentsTable').classList.remove('d-none');
                this.renderAppointments();
            }
        } catch (error) {
            console.error('Error loading appointments:', error);
            AppointmentUtils.showToast('Không thể tải danh sách lịch hẹn. Vui lòng thử lại.', 'error');
        } finally {
            loadingSpinner.classList.add('d-none');
        }
    },

    renderAppointments() {
        const tableBody = document.getElementById('appointmentsTableBody');
        tableBody.innerHTML = '';

        const filteredAppointments = this.currentFilter
            ? this.appointments.filter(apt => apt.Status === this.currentFilter)
            : this.appointments;

        filteredAppointments.forEach(appointment => {
            const row = this.createAppointmentRow(appointment);
            tableBody.appendChild(row);
        });

        if (filteredAppointments.length === 0) {
            const emptyRow = document.createElement('tr');
            emptyRow.innerHTML = `
                <td colspan="7" class="text-center text-muted py-4">
                    <i class="fas fa-search fa-2x mb-2"></i>
                    <p>Không tìm thấy lịch hẹn nào với bộ lọc hiện tại.</p>
                </td>
            `;
            tableBody.appendChild(emptyRow);
        }
    },

    createAppointmentRow(appointment) {
        const row = document.createElement('tr');

        let personInfo = '';
        let actionButtons = '';

        if (this.currentRole === 'Member') {
            personInfo = `
                <td>
                    <div class="d-flex align-items-center">
                        <div>
                            <div class="fw-bold">${appointment.Consultant?.User?.FullName || 'Không xác định'}</div>
                        </div>
                    </div>
                </td>
                <td>${appointment.Consultant?.Expertise || 'Chưa cập nhật'}</td>
            `;

            actionButtons = this.getMemberActionButtons(appointment);
        } else if (this.currentRole === 'Consultant') {
            personInfo = `
                <td>
                    <div class="d-flex align-items-center">
                        <img src="${appointment.User?.AvatarUrl || '/images/default-avatar.png'}" 
                             alt="Avatar" 
                             class="rounded-circle me-2" 
                             style="width: 40px; height: 40px; object-fit: cover;"
                             onerror="this.src='/images/default-avatar.png'">
                        <div>
                            <div class="fw-bold">${appointment.User?.FullName || 'Không xác định'}</div>
                        </div>
                    </div>
                </td>
                <td>${appointment.User?.Email || 'Không xác định'}</td>
            `;

            actionButtons = this.getConsultantActionButtons(appointment);
        }

        // Generate time slot display from duration for backward compatibility
        const timeSlotDisplay = appointment.DurationMinutes
            ? `${appointment.DurationMinutes} phút`
            : 'Chưa xác định';

        row.innerHTML = `
            ${personInfo}
            <td>${AppointmentUtils.formatDate(appointment.AppointmentDate)}</td>
            <td>
                <span class="badge bg-info text-dark">
                    <i class="fas fa-clock me-1"></i>
                    ${timeSlotDisplay}
                </span>
            </td>
            <td>${AppointmentUtils.getStatusBadge(appointment.Status)}</td>
            <td>
                <small class="text-muted">
                    ${appointment.Notes ? appointment.Notes.substring(0, 50) + (appointment.Notes.length > 50 ? '...' : '') : 'Không có ghi chú'}
                </small>
            </td>
            <td>${actionButtons}</td>
        `;

        return row;
    },

    getMemberActionButtons(appointment) {
        if (appointment.Status === 'Pending' || appointment.Status === 'Confirmed') {
            return `
                <button class="btn btn-sm btn-outline-danger btn-cancel-appointment" 
                        data-appointment-id="${appointment.AppointmentId}"
                        data-action="cancel">
                    <i class="fas fa-times"></i>
                    Hủy
                </button>
            `;
        }
        return '<span class="text-muted">Không có thao tác</span>';
    },

    getConsultantActionButtons(appointment) {
        const buttons = [];

        if (appointment.Status === 'Pending') {
            buttons.push(`
                <button class="btn btn-sm btn-success btn-confirm-appointment me-1" 
                        data-appointment-id="${appointment.AppointmentId}"
                        data-action="confirm">
                    <i class="fas fa-check"></i>
                    Xác nhận
                </button>
            `);
        }

        if (appointment.Status === 'Confirmed') {
            buttons.push(`
                <button class="btn btn-sm btn-primary btn-complete-appointment me-1" 
                        data-appointment-id="${appointment.AppointmentId}"
                        data-action="complete">
                    <i class="fas fa-check-double"></i>
                    Hoàn thành
                </button>
            `);
        }

        if (appointment.Status === 'Pending' || appointment.Status === 'Confirmed') {
            buttons.push(`
                <button class="btn btn-sm btn-outline-danger btn-cancel-appointment" 
                        data-appointment-id="${appointment.AppointmentId}"
                        data-action="cancel">
                    <i class="fas fa-times"></i>
                    Hủy
                </button>
            `);
        }

        return buttons.length > 0 ? buttons.join('') : '<span class="text-muted">Không có thao tác</span>';
    },

    bindEvents() {
        document.querySelectorAll('input[name="statusFilter"]').forEach(radio => {
            radio.addEventListener('change', (e) => {
                this.currentFilter = e.target.value;
                this.renderAppointments();
            });
        });

        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-confirm-appointment')) {
                const btn = e.target.closest('.btn-confirm-appointment');
                this.showConfirmModal(btn.dataset.appointmentId, 'confirm', 'Bạn có chắc chắn muốn xác nhận lịch hẹn này?');
            }

            if (e.target.closest('.btn-complete-appointment')) {
                const btn = e.target.closest('.btn-complete-appointment');
                this.showConfirmModal(btn.dataset.appointmentId, 'complete', 'Bạn có chắc chắn muốn đánh dấu lịch hẹn này là hoàn thành?');
            }

            if (e.target.closest('.btn-cancel-appointment')) {
                const btn = e.target.closest('.btn-cancel-appointment');
                this.showConfirmModal(btn.dataset.appointmentId, 'cancel', 'Bạn có chắc chắn muốn hủy lịch hẹn này?');
            }
        });

        document.getElementById('confirmAction')?.addEventListener('click', () => {
            this.executeAction();
        });
    },

    showConfirmModal(appointmentId, action, message) {
        this.pendingAction = { appointmentId, action };

        document.getElementById('confirmMessage').textContent = message;

        const modal = new bootstrap.Modal(document.getElementById('confirmModal'));
        modal.show();
    },

    async executeAction() {
        if (!this.pendingAction) return;

        const { appointmentId, action } = this.pendingAction;
        const confirmBtn = document.getElementById('confirmAction');

        try {
            confirmBtn.disabled = true;
            confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';

            let updateData = {};
            let successMessage = '';

            switch (action) {
                case 'confirm':
                    updateData = { Status: 'Confirmed' };
                    successMessage = 'Xác nhận lịch hẹn thành công!';
                    break;
                case 'complete':
                    updateData = { Status: 'Completed' };
                    successMessage = 'Đánh dấu hoàn thành lịch hẹn thành công!';
                    break;
                case 'cancel':
                    updateData = { Status: 'Cancelled' };
                    successMessage = 'Hủy lịch hẹn thành công!';
                    break;
            }

            const response = await AppointmentAPI.updateAppointment(appointmentId, updateData);

            // Handle OData PATCH response - might not have explicit success field
            if (response && !response.error) {
                AppointmentUtils.showToast(successMessage, 'success');

                const modal = bootstrap.Modal.getInstance(document.getElementById('confirmModal'));
                modal.hide();

                await this.loadAppointments();
            } else {
                throw new Error(response?.message || response?.error?.message || 'Không thể cập nhật lịch hẹn');
            }

        } catch (error) {
            console.error('Error updating appointment:', error);
            let errorMessage = 'Không thể cập nhật lịch hẹn. Vui lòng thử lại.';

            if (error.message) {
                errorMessage = error.message;
            } else if (typeof error === 'string') {
                errorMessage = error;
            }

            AppointmentUtils.showToast(errorMessage, 'error');
        } finally {
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = 'Xác nhận';
            this.pendingAction = null;
        }
    }
};