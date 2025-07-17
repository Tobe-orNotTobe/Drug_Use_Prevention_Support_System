const AdminModule = {
    apiBaseUrl: 'https://localhost:7008',
    authToken: null,
    currentUserId: null,
    currentPage: 1,
    pageSize: 10,

    init() {
        this.authToken = document.querySelector('meta[name="auth-token"]')?.getAttribute('content');
        this.currentUserId = document.querySelector('meta[name="current-user-id"]')?.getAttribute('content');
        this.apiBaseUrl = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || this.apiBaseUrl;

        this.bindEvents();
        this.loadPageData();
    },

    bindEvents() {
        document.getElementById('exportReportBtn')?.addEventListener('click', () => this.exportDashboard());
        document.getElementById('searchBtn')?.addEventListener('click', () => this.searchUsers());
        document.getElementById('saveUserBtn')?.addEventListener('click', () => this.saveUser());
        document.getElementById('updateUserBtn')?.addEventListener('click', () => this.updateUser());
        document.getElementById('confirmDeleteBtn')?.addEventListener('click', () => this.deleteUser());

        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('btn-view-user')) {
                this.viewUserDetails(e.target.dataset.userId);
            }
            if (e.target.classList.contains('btn-edit-user')) {
                this.editUser(e.target.dataset.userId);
            }
            if (e.target.classList.contains('btn-delete-user')) {
                this.confirmDeleteUser(e.target.dataset.userId, e.target.dataset.userName);
            }
        });

        const searchInputs = ['searchFullName', 'searchEmail', 'filterRole', 'filterStatus'];
        searchInputs.forEach(id => {
            const element = document.getElementById(id);
            if (element) {
                element.addEventListener('keypress', (e) => {
                    if (e.key === 'Enter') this.searchUsers();
                });
            }
        });
    },

    loadPageData() {
        const currentPath = window.location.pathname.toLowerCase();

        if (currentPath.includes('dashboard')) {
            this.loadDashboardData();
        } else if (currentPath.includes('usermanagement') || currentPath.includes('users')) {
            this.loadUsers();
        }
    },

    async makeRequest(url, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${this.authToken}`
            }
        };

        const mergedOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, mergedOptions);

            if (!response.ok) {
                const errorText = await response.text();
                let errorMessage = 'Có lỗi xảy ra';

                try {
                    const errorData = JSON.parse(errorText);
                    errorMessage = errorData.message || errorMessage;
                } catch {
                    errorMessage = errorText || errorMessage;
                }

                throw new Error(errorMessage);
            }

            const data = await response.json();
            return data;
        } catch (error) {
            this.showToast('error', error.message);
            throw error;
        }
    },

    async loadDashboardData() {
        try {
            await Promise.all([
                this.loadDashboardStats(),
                this.loadUsersByRoleChart(),
                this.loadAppointmentsByStatusChart(),
            ]);
        } catch (error) {
            console.error('Error loading dashboard data:', error);
        }
    },

    async loadDashboardStats() {
        try {
            const stats = await this.makeRequest(`${this.apiBaseUrl}/api/Dashboard/stats`);

            if (stats.success && stats.data) {
                const data = stats.data;
                document.getElementById('totalUsers').textContent = data.users?.totalUsers || 0;
                document.getElementById('totalConsultants').textContent = data.consultants?.totalConsultants || 0;
                document.getElementById('totalAppointments').textContent = data.appointments?.totalAppointments || 0;
                document.getElementById('totalCourses').textContent = data.courses?.totalCourses || 0;
                document.getElementById('totalSurveys').textContent = data.surveys?.totalSurveys || 0;
            }
        } catch (error) {
            console.error('Error loading dashboard stats:', error);
            const elements = ['totalUsers', 'totalConsultants', 'totalAppointments', 'totalCourses', 'totalSurveys'];
            elements.forEach(id => {
                const element = document.getElementById(id);
                if (element) element.textContent = '0';
            });
        }
    },

    async loadUsersByRoleChart() {
        try {
            const stats = await this.makeRequest(`${this.apiBaseUrl}/api/Dashboard/stats`);

            const ctx = document.getElementById('usersByRoleChart');
            if (ctx && stats.success && stats.data?.users?.roleCounts) {
                const roleCounts = stats.data.users.roleCounts;
                new Chart(ctx, {
                    type: 'pie',
                    data: {
                        labels: ['Member', 'Staff', 'Consultant', 'Manager', 'Admin'],
                        datasets: [{
                            data: [
                                roleCounts.member || 0,
                                roleCounts.staff || 0,
                                roleCounts.consultant || 0,
                                roleCounts.manager || 0,
                                roleCounts.admin || 0
                            ],
                            backgroundColor: [
                                '#4e73df',
                                '#1cc88a',
                                '#36b9cc',
                                '#f6c23e',
                                '#e74a3b'
                            ]
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'bottom'
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading users by role chart:', error);
        }
    },

    async loadAppointmentsByStatusChart() {
        try {
            const stats = await this.makeRequest(`${this.apiBaseUrl}/api/Dashboard/stats`);

            const ctx = document.getElementById('appointmentsByStatusChart');
            if (ctx && stats.success && stats.data?.appointments?.statusCounts) {
                const statusCounts = stats.data.appointments.statusCounts;
                new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: ['Pending', 'Confirmed', 'Completed', 'Cancelled'],
                        datasets: [{
                            label: 'Số lượng',
                            data: [
                                statusCounts.pending || 0,
                                statusCounts.confirmed || 0,
                                statusCounts.completed || 0,
                                statusCounts.cancelled || 0
                            ],
                            backgroundColor: [
                                '#f6c23e',
                                '#1cc88a',
                                '#36b9cc',
                                '#e74a3b'
                            ]
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }
                });
            }
        } catch (error) {
            console.error('Error loading appointments by status chart:', error);
        }
    },

    async exportDashboard() {
        const exportBtn = document.getElementById('exportReportBtn');
        const originalText = exportBtn.innerHTML;

        try {
            exportBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xuất...';
            exportBtn.disabled = true;

            this.showToast('info', 'Đang xuất báo cáo...');

            const response = await fetch(`${this.apiBaseUrl}/api/ExcelExport/dashboard`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `Dashboard_Report_${new Date().getTime()}.xlsx`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);

                this.showToast('success', 'Xuất báo cáo thành công!');
            } else {
                throw new Error('Không thể xuất báo cáo');
            }
        } catch (error) {
            this.showToast('error', error.message);
        } finally {
            exportBtn.innerHTML = originalText;
            exportBtn.disabled = false;
        }
    },

    async loadUsers(page = 1) {
        try {
            const searchParams = this.getSearchParams();
            const skip = (page - 1) * this.pageSize;

            let filterQuery = '';
            const filters = [];

            if (searchParams.fullName) {
                filters.push(`contains(tolower(FullName), '${searchParams.fullName.toLowerCase()}')`);
            }
            if (searchParams.email) {
                filters.push(`contains(tolower(Email), '${searchParams.email.toLowerCase()}')`);
            }
            if (searchParams.status) {
                const isActive = searchParams.status === 'Active';
                filters.push(`IsActive eq ${isActive}`);
            }

            if (filters.length > 0) {
                filterQuery = `&$filter=${filters.join(' and ')}`;
            }

            const response = await fetch(`${this.apiBaseUrl}/odata/Users?$expand=Roles&$count=true&$skip=${skip}&$top=${this.pageSize}&$orderby=CreatedAt desc${filterQuery}`, {
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Không thể tải danh sách users');
            }

            const data = await response.json();
            this.renderUsersTable(data.value || []);
            this.renderPagination(data['@odata.count'] || 0, page);
            this.currentPage = page;
        } catch (error) {
            console.error('Error loading users:', error);
            this.renderUsersTable([]);
            this.showToast('error', 'Lỗi tải danh sách users');
        }
    },

    getSearchParams() {
        return {
            fullName: document.getElementById('searchFullName')?.value || '',
            email: document.getElementById('searchEmail')?.value || '',
            role: document.getElementById('filterRole')?.value || '',
            status: document.getElementById('filterStatus')?.value || ''
        };
    },

    renderUsersTable(users) {
        const tbody = document.getElementById('usersTableBody');
        if (!tbody) return;

        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="8" class="text-center">Không có dữ liệu</td></tr>';
            return;
        }

        tbody.innerHTML = users.map(user => {
            const userRoles = user.Roles ? user.Roles.map(role => role.RoleName).join(', ') : 'Member';
            const primaryRole = user.Roles && user.Roles.length > 0 ? user.Roles[0].RoleName : 'Member';
            const status = user.IsActive ? 'Active' : 'Inactive';

            return `
            <tr>
                <td>${user.UserId}</td>
                <td>${user.FullName}</td>
                <td>${user.Email}</td>
                <td>${user.Phone || ''}</td>
                <td><span class="badge ${this.getRoleBadgeClass(primaryRole)}">${userRoles}</span></td>
                <td><span class="badge ${this.getStatusBadgeClass(status)}">${status}</span></td>
                <td>${this.formatDate(user.CreatedAt)}</td>
                <td>
                    <div class="btn-group btn-group-sm" role="group">
                        <button type="button" class="btn btn-info btn-sm btn-view-user" data-user-id="${user.UserId}" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button type="button" class="btn btn-warning btn-sm btn-edit-user" data-user-id="${user.UserId}" title="Chỉnh sửa" ${this.canEditUser(user) ? '' : 'disabled'}>
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-danger btn-sm btn-delete-user" data-user-id="${user.UserId}" data-user-name="${user.FullName}" title="Xóa" ${this.canDeleteUser(user) ? '' : 'disabled'}>
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
        }).join('');
    },

    renderPagination(totalCount, currentPage) {
        const pagination = document.getElementById('usersPagination');
        if (!pagination) return;

        const totalPages = Math.ceil(totalCount / this.pageSize);

        if (totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let paginationHtml = '';

        paginationHtml += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="AdminModule.loadUsers(${currentPage - 1})" tabindex="-1">Trước</a>
        </li>`;

        for (let i = Math.max(1, currentPage - 2); i <= Math.min(totalPages, currentPage + 2); i++) {
            paginationHtml += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="AdminModule.loadUsers(${i})">${i}</a>
            </li>`;
        }

        paginationHtml += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
            <a class="page-link" href="#" onclick="AdminModule.loadUsers(${currentPage + 1})">Sau</a>
        </li>`;

        pagination.innerHTML = paginationHtml;
    },

    searchUsers() {
        this.loadUsers(1);
    },

    async saveUser() {
        const form = document.getElementById('createUserForm');
        if (!this.validateForm(form)) return;

        const password = document.getElementById('createPassword').value;
        const confirmPassword = document.getElementById('createConfirmPassword').value;

        if (password !== confirmPassword) {
            this.showToast('error', 'Mật khẩu xác nhận không khớp');
            return;
        }

        const userData = {
            FullName: document.getElementById('createFullName').value,
            Email: document.getElementById('createEmail').value,
            PasswordHash: password,
            Phone: document.getElementById('createPhone').value || null,
            DateOfBirth: document.getElementById('createDateOfBirth').value || null,
            Gender: document.getElementById('createGender').value || null,
            Address: document.getElementById('createAddress').value || null,
            IsActive: document.getElementById('createStatus').value === 'Active',
            RoleName: document.getElementById('createRole').value
        };

        try {
            const response = await fetch(`${this.apiBaseUrl}/odata/Users`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(userData)
            });

            if (response.ok) {
                this.showToast('success', 'Tạo user thành công!');
                bootstrap.Modal.getInstance(document.getElementById('createUserModal')).hide();
                form.reset();
                this.loadUsers(this.currentPage);
            } else {
                const errorText = await response.text();
                let errorMessage = 'Có lỗi xảy ra khi tạo user';
                try {
                    const error = JSON.parse(errorText);
                    errorMessage = error.message || errorMessage;
                } catch {
                    errorMessage = errorText || errorMessage;
                }
                throw new Error(errorMessage);
            }
        } catch (error) {
            console.error('Error creating user:', error);
            this.showToast('error', error.message);
        }
    },

    async updateUser() {
        const form = document.getElementById('editUserForm');
        if (!this.validateForm(form)) return;

        const userId = document.getElementById('editUserId').value;
        const userData = {
            UserId: parseInt(userId),
            FullName: document.getElementById('editFullName').value,
            Email: document.getElementById('editEmail').value,
            Phone: document.getElementById('editPhone').value,
            Role: document.getElementById('editRole').value,
            Status: document.getElementById('editStatus').value
        };

        try {
            const response = await fetch(`${this.apiBaseUrl}/odata/Users(${userId})`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(userData)
            });

            if (response.ok) {
                this.showToast('success', 'Cập nhật user thành công!');
                bootstrap.Modal.getInstance(document.getElementById('editUserModal')).hide();
                this.loadUsers(this.currentPage);
            } else {
                const error = await response.json();
                throw new Error(error.message || 'Có lỗi xảy ra khi cập nhật user');
            }
        } catch (error) {
            console.error('Error updating user:', error);
            this.showToast('error', error.message);
        }
    },

    confirmDeleteUser(userId, userName) {
        document.getElementById('deleteUserId').value = userId;
        document.getElementById('deleteUserName').textContent = userName;
        new bootstrap.Modal(document.getElementById('deleteUserModal')).show();
    },

    async deleteUser() {
        const userId = document.getElementById('deleteUserId').value;

        try {
            const response = await fetch(`${this.apiBaseUrl}/odata/Users(${userId})`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.showToast('success', 'Xóa user thành công!');
                bootstrap.Modal.getInstance(document.getElementById('deleteUserModal')).hide();
                this.loadUsers(this.currentPage);
            } else {
                const error = await response.json();
                throw new Error(error.message || 'Có lỗi xảy ra khi xóa user');
            }
        } catch (error) {
            console.error('Error deleting user:', error);
            this.showToast('error', error.message);
        }
    },

    async viewUserDetails(userId) {
        try {
            const response = await fetch(`${this.apiBaseUrl}/odata/Users(${userId})`, {
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Không thể tải thông tin user');
            }

            const user = await response.json();

            document.getElementById('detailUserId').textContent = user.UserId;
            document.getElementById('detailFullName').textContent = user.FullName;
            document.getElementById('detailEmail').textContent = user.Email;
            document.getElementById('detailPhone').textContent = user.Phone || 'Chưa có';

            const roleSpan = document.getElementById('detailRole');
            roleSpan.textContent = user.Role;
            roleSpan.className = `badge ${this.getRoleBadgeClass(user.Role)}`;

            const statusSpan = document.getElementById('detailStatus');
            statusSpan.textContent = user.Status;
            statusSpan.className = `badge ${this.getStatusBadgeClass(user.Status)}`;

            document.getElementById('detailCreatedDate').textContent = this.formatDateTime(user.CreatedAt);
            document.getElementById('detailLastLogin').textContent = user.LastLoginAt ? this.formatDateTime(user.LastLoginAt) : 'Chưa đăng nhập';

            new bootstrap.Modal(document.getElementById('userDetailModal')).show();
        } catch (error) {
            console.error('Error loading user details:', error);
            this.showToast('error', error.message);
        }
    },

    validateForm(form) {
        const inputs = form.querySelectorAll('input[required], select[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
            }
        });

        return isValid;
    },

    canEditUser(user) {
        return user.UserId.toString() !== this.currentUserId || user.Role !== 'Admin';
    },

    canDeleteUser(user) {
        return user.UserId.toString() !== this.currentUserId;
    },

    getRoleBadgeClass(role) {
        const classes = {
            'Admin': 'bg-danger',
            'Manager': 'bg-warning',
            'Staff': 'bg-info',
            'Consultant': 'bg-success',
            'Member': 'bg-primary'
        };
        return classes[role] || 'bg-secondary';
    },

    getStatusBadgeClass(status) {
        return status === 'Active' ? 'bg-success' : 'bg-secondary';
    },

    formatDate(dateString) {
        if (!dateString) return '';
        return new Date(dateString).toLocaleDateString('vi-VN');
    },

    formatDateTime(dateString) {
        if (!dateString) return '';
        return new Date(dateString).toLocaleString('vi-VN');
    },

    showToast(type, message) {
        const toastElement = document.getElementById('actionToast') || document.getElementById('exportToast');
        if (!toastElement) return;

        const toastBody = toastElement.querySelector('.toast-body span');
        const toastIcon = toastElement.querySelector('.toast-header i');

        if (toastBody) toastBody.textContent = message;

        if (toastIcon) {
            toastIcon.className = `fas me-2 ${this.getToastIcon(type)}`;
        }

        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    },

    getToastIcon(type) {
        const icons = {
            'success': 'fa-check-circle text-success',
            'error': 'fa-exclamation-circle text-danger',
            'warning': 'fa-exclamation-triangle text-warning',
            'info': 'fa-info-circle text-info'
        };
        return icons[type] || 'fa-info-circle text-info';
    }
};

document.addEventListener('DOMContentLoaded', () => {
    AdminModule.init();
});