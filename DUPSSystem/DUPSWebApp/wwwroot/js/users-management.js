$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata';
    const USER_PERMISSIONS = window.USER_PERMISSIONS || {};

    let currentPage = 1;
    let pageSize = 20;
    let totalRecords = 0;

    // Security check - Only admin can access
    if (!USER_PERMISSIONS.isAdmin) {
        showAlert('error', 'Bạn không có quyền truy cập trang này');
        setTimeout(() => {
            window.location.href = '/Home/Index';
        }, 2000);
        return;
    }

    // Initialize page
    init();

    function init() {
        loadUsers();
        loadUserStatistics();
        bindEvents();
    }

    function bindEvents() {
        // Search and filter functionality
        $('#searchBtn').click(function () {
            currentPage = 1;
            loadUsers();
        });

        $('#searchInput').keypress(function (e) {
            if (e.which === 13) {
                currentPage = 1;
                loadUsers();
            }
        });

        $('#roleFilter, #statusFilter, #dateFromFilter, #dateToFilter').change(function () {
            currentPage = 1;
            loadUsers();
        });

        // Form submissions
        $('#addUserForm').submit(function (e) {
            e.preventDefault();
            addUser();
        });

        $('#editUserForm').submit(function (e) {
            e.preventDefault();
            updateUser();
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        return {
            'Authorization': `Bearer ${window.USER_TOKEN}`,
            'Content-Type': 'application/json'
        };
    }

    // Load users with filtering and pagination
    function loadUsers() {
        showLoading();

        let odataQuery = `${API_BASE_URL}/Users?`;
        let filters = [];

        // Search filter
        const searchTerm = $('#searchInput').val().trim();
        if (searchTerm) {
            filters.push(`contains(tolower(FullName), '${searchTerm.toLowerCase()}') or contains(tolower(Email), '${searchTerm.toLowerCase()}')`);
        }

        // Role filter
        const role = $('#roleFilter').val();
        if (role) {
            filters.push(`Role eq '${role}'`);
        }

        // Status filter
        const status = $('#statusFilter').val();
        if (status !== '') {
            filters.push(`IsActive eq ${status}`);
        }

        // Date range filter
        const dateFrom = $('#dateFromFilter').val();
        const dateTo = $('#dateToFilter').val();
        if (dateFrom) {
            filters.push(`CreatedAt ge ${dateFrom}T00:00:00Z`);
        }
        if (dateTo) {
            filters.push(`CreatedAt le ${dateTo}T23:59:59Z`);
        }

        // Apply filters
        if (filters.length > 0) {
            odataQuery += `$filter=${filters.join(' and ')}&`;
        }

        // Add ordering and pagination
        odataQuery += `$orderby=CreatedAt desc&$top=${pageSize}&$skip=${(currentPage - 1) * pageSize}&$count=true`;

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                hideLoading();
                const users = response.value || response;
                displayUsers(users);
                totalRecords = response['@odata.count'] || users.length;
                updatePagination();
            },
            error: function (xhr, status, error) {
                hideLoading();
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi tải danh sách người dùng');
            }
        });
    }

    // Display users in table
    function displayUsers(users) {
        const tbody = $('#usersTableBody');
        tbody.empty();

        if (!users || users.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="8" class="text-center">Không có người dùng nào</td>
                </tr>
            `);
            return;
        }

        users.forEach(user => {
            const actionButtons = generateUserActionButtons(user);

            const row = `
                <tr>
                    <td>${user.UserId}</td>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="avatar-circle me-2">
                                ${user.FullName ? user.FullName.charAt(0).toUpperCase() : 'U'}
                            </div>
                            <div>
                                <strong>${escapeHtml(user.FullName || 'N/A')}</strong>
                                ${user.PhoneNumber ? `<br><small class="text-muted">${escapeHtml(user.PhoneNumber)}</small>` : ''}
                            </div>
                        </div>
                    </td>
                    <td>
                        <a href="mailto:${user.Email}">${escapeHtml(user.Email)}</a>
                        ${user.EmailConfirmed ? '<i class="fas fa-check-circle text-success ms-1" title="Email đã xác thực"></i>' : '<i class="fas fa-exclamation-triangle text-warning ms-1" title="Email chưa xác thực"></i>'}
                    </td>
                    <td>
                        <span class="badge bg-${getRoleClass(user.Role)}">
                            ${user.Role}
                        </span>
                    </td>
                    <td>
                        <span class="badge bg-${user.IsActive ? 'success' : 'danger'}">
                            ${user.IsActive ? 'Hoạt động' : 'Bị khóa'}
                        </span>
                    </td>
                    <td>${formatDate(user.CreatedAt)}</td>
                    <td>${formatDate(user.LastLoginAt) || 'Chưa đăng nhập'}</td>
                    <td>${actionButtons}</td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Generate action buttons for users
    function generateUserActionButtons(user) {
        let buttons = '';

        // View user details
        buttons += `<button type="button" class="btn btn-info btn-sm me-1" onclick="viewUser(${user.UserId})" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button>`;

        // Edit user (Admin can edit all users)
        buttons += `<button type="button" class="btn btn-warning btn-sm me-1" onclick="editUser(${user.UserId})" title="Chỉnh sửa">
                        <i class="fas fa-edit"></i>
                    </button>`;

        // Lock/Unlock user
        if (user.IsActive) {
            buttons += `<button type="button" class="btn btn-danger btn-sm me-1" onclick="lockUser(${user.UserId})" title="Khóa tài khoản">
                            <i class="fas fa-lock"></i>
                        </button>`;
        } else {
            buttons += `<button type="button" class="btn btn-success btn-sm me-1" onclick="unlockUser(${user.UserId})" title="Mở khóa tài khoản">
                            <i class="fas fa-unlock"></i>
                        </button>`;
        }

        // Reset password
        buttons += `<button type="button" class="btn btn-secondary btn-sm me-1" onclick="resetPassword(${user.UserId})" title="Reset mật khẩu">
                        <i class="fas fa-key"></i>
                    </button>`;

        // Delete user (careful with this one)
        if (user.Role !== 'Admin' || user.UserId !== window.CURRENT_USER_ID) {
            buttons += `<button type="button" class="btn btn-dark btn-sm" onclick="deleteUser(${user.UserId})" title="Xóa người dùng">
                            <i class="fas fa-trash"></i>
                        </button>`;
        }

        return `<div class="btn-group btn-group-sm" role="group">${buttons}</div>`;
    }

    // Load user statistics
    function loadUserStatistics() {
        // Total users
        $.ajax({
            url: `${API_BASE_URL}/Users?$count=true`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                $('#totalUsers').text(response['@odata.count'] || 0);
            }
        });

        // Count by role
        const roles = ['Member', 'Staff', 'Consultant', 'Manager', 'Admin'];
        roles.forEach(role => {
            $.ajax({
                url: `${API_BASE_URL}/Users?$filter=Role eq '${role}'&$count=true`,
                method: 'GET',
                headers: setupAjaxHeaders(),
                success: function (response) {
                    $(`#total${role}s`).text(response['@odata.count'] || 0);
                }
            });
        });
    }

    // User action functions
    window.viewUser = function (userId) {
        window.location.href = `/Users/Details/${userId}`;
    };

    window.editUser = function (userId) {
        // Load user data into edit modal
        $.ajax({
            url: `${API_BASE_URL}/Users(${userId})`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (user) {
                $('#editUserId').val(user.UserId);
                $('#editFullName').val(user.FullName);
                $('#editEmail').val(user.Email);
                $('#editPhoneNumber').val(user.PhoneNumber);
                $('#editDateOfBirth').val(user.DateOfBirth ? user.DateOfBirth.split('T')[0] : '');
                $('#editRole').val(user.Role);
                $('#editIsActive').val(user.IsActive.toString());
                $('#resetPassword').prop('checked', false);

                $('#editUserModal').modal('show');
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi tải thông tin người dùng');
            }
        });
    };

    window.lockUser = function (userId) {
        if (confirm('Bạn có chắc chắn muốn khóa tài khoản này?')) {
            updateUserStatus(userId, false);
        }
    };

    window.unlockUser = function (userId) {
        if (confirm('Bạn có chắc chắn muốn mở khóa tài khoản này?')) {
            updateUserStatus(userId, true);
        }
    };

    window.resetPassword = function (userId) {
        if (confirm('Bạn có chắc chắn muốn reset mật khẩu cho người dùng này?')) {
            resetUserPassword(userId);
        }
    };

    window.deleteUser = function (userId) {
        if (confirm('Bạn có chắc chắn muốn xóa người dùng này? Hành động này không thể hoàn tác!')) {
            deleteUserAction(userId);
        }
    };

    window.exportUsers = function () {
        // Export users to Excel/CSV
        showAlert('info', 'Đang xuất dữ liệu...');

        $.ajax({
            url: `${API_BASE_URL}/Users?$orderby=CreatedAt desc`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const users = response.value || response;
                exportToCSV(users);
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi xuất dữ liệu');
            }
        });
    };

    // Action implementations
    window.addUser = function () {
        const userData = {
            FullName: $('#fullName').val().trim(),
            Email: $('#email').val().trim(),
            PhoneNumber: $('#phoneNumber').val().trim(),
            DateOfBirth: $('#dateOfBirth').val() || null,
            Role: $('#role').val(),
            IsActive: $('#isActive').val() === 'true',
            Password: $('#password').val(),
            CreatedAt: new Date().toISOString()
        };

        // Validation
        if (!userData.FullName || !userData.Email || !userData.Role || !userData.Password) {
            showAlert('error', 'Vui lòng điền đầy đủ các trường bắt buộc');
            return;
        }

        $.ajax({
            url: `${API_BASE_URL}/Users`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(userData),
            success: function (response) {
                $('#addUserModal').modal('hide');
                $('#addUserForm')[0].reset();
                showAlert('success', 'Thêm người dùng thành công!');
                loadUsers();
                loadUserStatistics();
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi thêm người dùng');
            }
        });
    };

    window.updateUser = function () {
        const userId = $('#editUserId').val();
        const userData = {
            FullName: $('#editFullName').val().trim(),
            Email: $('#editEmail').val().trim(),
            PhoneNumber: $('#editPhoneNumber').val().trim(),
            DateOfBirth: $('#editDateOfBirth').val() || null,
            Role: $('#editRole').val(),
            IsActive: $('#editIsActive').val() === 'true',
            UpdatedAt: new Date().toISOString()
        };

        // If reset password is checked
        if ($('#resetPassword').prop('checked')) {
            userData.ResetPassword = true;
        }

        $.ajax({
            url: `${API_BASE_URL}/Users(${userId})`,
            method: 'PATCH',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(userData),
            success: function (response) {
                $('#editUserModal').modal('hide');
                showAlert('success', 'Cập nhật người dùng thành công!');
                loadUsers();
                loadUserStatistics();
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi cập nhật người dùng');
            }
        });
    };

    function updateUserStatus(userId, isActive) {
        const data = {
            IsActive: isActive,
            UpdatedAt: new Date().toISOString()
        };

        $.ajax({
            url: `${API_BASE_URL}/Users(${userId})`,
            method: 'PATCH',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(data),
            success: function (response) {
                showAlert('success', `${isActive ? 'Mở khóa' : 'Khóa'} tài khoản thành công!`);
                loadUsers();
                loadUserStatistics();
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi cập nhật trạng thái người dùng');
            }
        });
    }

    function resetUserPassword(userId) {
        const data = {
            ResetPassword: true,
            UpdatedAt: new Date().toISOString()
        };

        $.ajax({
            url: `${API_BASE_URL}/Users(${userId})/ResetPassword`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(data),
            success: function (response) {
                showAlert('success', 'Reset mật khẩu thành công! Mật khẩu mới đã được gửi qua email.');
                loadUsers();
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi reset mật khẩu');
            }
        });
    }

    function deleteUserAction(userId) {
        $.ajax({
            url: `${API_BASE_URL}/Users(${userId})`,
            method: 'DELETE',
            headers: setupAjaxHeaders(),
            success: function (response) {
                showAlert('success', 'Xóa người dùng thành công!');
                loadUsers();
                loadUserStatistics();
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi xóa người dùng');
            }
        });
    }

    function exportToCSV(users) {
        const csvContent = "data:text/csv;charset=utf-8,"
            + "ID,Họ tên,Email,Số điện thoại,Vai trò,Trạng thái,Ngày tạo,Đăng nhập cuối\n"
            + users.map(user =>
                `${user.UserId},"${user.FullName || ''}","${user.Email}","${user.PhoneNumber || ''}","${user.Role}","${user.IsActive ? 'Hoạt động' : 'Bị khóa'}","${formatDate(user.CreatedAt)}","${formatDate(user.LastLoginAt) || 'Chưa đăng nhập'}"`
            ).join("\n");

        const encodedUri = encodeURI(csvContent);
        const link = document.createElement("a");
        link.setAttribute("href", encodedUri);
        link.setAttribute("download", `users_export_${new Date().toISOString().split('T')[0]}.csv`);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        showAlert('success', 'Xuất dữ liệu thành công!');
    }

    // Helper functions
    function getRoleClass(role) {
        const classes = {
            'Member': 'success',
            'Staff': 'info',
            'Consultant': 'warning',
            'Manager': 'secondary',
            'Admin': 'danger'
        };
        return classes[role] || 'primary';
    }

    function formatDate(dateString) {
        if (!dateString) return null;
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN');
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
                window.location.href = '/Auth/Login';
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
        loadUsers();
    };

    // Add custom CSS for avatar circle
    $('<style>')
        .prop('type', 'text/css')
        .html(`
            .avatar-circle {
                width: 40px;
                height: 40px;
                border-radius: 50%;
                background-color: #007bff;
                color: white;
                display: flex;
                align-items: center;
                justify-content: center;
                font-weight: bold;
                font-size: 16px;
            }
        `)
        .appendTo('head');
});