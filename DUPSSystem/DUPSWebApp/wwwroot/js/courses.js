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
        loadCourses();
        if (USER_PERMISSIONS.isAuthenticated) {
            loadUserCourses();
        }
        bindEvents();
    }

    function bindEvents() {
        // Search functionality
        $('#searchBtn').click(function () {
            currentPage = 1;
            loadCourses();
        });

        $('#searchInput').keypress(function (e) {
            if (e.which === 13) { // Enter key
                currentPage = 1;
                loadCourses();
            }
        });

        // Filter functionality
        $('#targetAudienceFilter, #statusFilter').change(function () {
            currentPage = 1;
            loadCourses();
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

    // Load all courses with filtering and pagination
    function loadCourses() {
        showLoading();

        let odataQuery = `${API_BASE_URL}/Courses?`;
        let filters = [];

        // Search filter
        const searchTerm = $('#searchInput').val().trim();
        if (searchTerm) {
            filters.push(`contains(tolower(Title), '${searchTerm.toLowerCase()}') or contains(tolower(Description), '${searchTerm.toLowerCase()}')`);
        }

        // Target audience filter
        const targetAudience = $('#targetAudienceFilter').val();
        if (targetAudience) {
            filters.push(`TargetAudience eq '${targetAudience}'`);
        }

        // Status filter
        const status = $('#statusFilter').val();
        if (status !== '') {
            filters.push(`IsActive eq ${status}`);
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
                const courses = response.value || response;
                displayCourses(courses);
                totalRecords = response['@odata.count'] || courses.length;
                updatePagination();
            },
            error: function (xhr, status, error) {
                hideLoading();
                console.error('Error loading courses:', error);
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi tải danh sách khóa học');
            }
        });
    }

    // Display courses in table with role-based action buttons
    function displayCourses(courses) {
        const tbody = $('#coursesTableBody');
        tbody.empty();

        if (!courses || courses.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="7" class="text-center">Không có khóa học nào</td>
                </tr>
            `);
            return;
        }

        courses.forEach(course => {
            const actionButtons = generateActionButtons(course);

            const row = `
                <tr>
                    <td>${course.CourseId}</td>
                    <td>
                        <strong>${escapeHtml(course.Title)}</strong>
                        ${course.Description ? `<br><small class="text-muted">${escapeHtml(course.Description.substring(0, 100))}${course.Description.length > 100 ? '...' : ''}</small>` : ''}
                    </td>
                    <td>${getTargetAudienceText(course.TargetAudience)}</td>
                    <td>${course.DurationMinutes || 'N/A'}</td>
                    <td>
                        <span class="badge bg-${course.IsActive ? 'success' : 'secondary'}">
                            ${course.IsActive ? 'Hoạt động' : 'Không hoạt động'}
                        </span>
                    </td>
                    <td>${formatDate(course.CreatedAt)}</td>
                    <td>${actionButtons}</td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Generate action buttons based on user permissions
    function generateActionButtons(course) {
        let buttons = '';

        // Everyone can view details
        buttons += `<button type="button" class="btn btn-info btn-sm me-1" onclick="viewCourse(${course.CourseId})" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button>`;

        // Only authenticated users can register
        if (USER_PERMISSIONS.isAuthenticated && USER_PERMISSIONS.canRegisterCourses) {
            buttons += `<button type="button" class="btn btn-success btn-sm me-1" onclick="registerForCourse(${course.CourseId})" title="Đăng ký">
                            <i class="fas fa-user-plus"></i>
                        </button>`;
        }

        // Only Staff+ can manage courses
        if (USER_PERMISSIONS.canManageCourses) {
            buttons += `<button type="button" class="btn btn-warning btn-sm me-1" onclick="editCourse(${course.CourseId})" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>`;

            buttons += `<button type="button" class="btn btn-danger btn-sm" onclick="deleteCourse(${course.CourseId})" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>`;
        }

        return `<div class="btn-group btn-group-sm" role="group">${buttons}</div>`;
    }

    // Load user's registered courses (only for authenticated users)
    function loadUserCourses() {
        if (!USER_PERMISSIONS.isAuthenticated || !CURRENT_USER_ID) {
            return;
        }

        const odataQuery = `${API_BASE_URL}/UserCourses?$filter=UserId eq ${CURRENT_USER_ID}&$expand=Course&$orderby=RegisteredAt desc`;

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const userCourses = response.value || response;
                displayUserCourses(userCourses);
            },
            error: function (xhr, status, error) {
                console.error('Error loading user courses:', error);
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi tải khóa học của bạn');
            }
        });
    }

    // Display user's registered courses
    function displayUserCourses(userCourses) {
        const tbody = $('#userCoursesTableBody');
        tbody.empty();

        if (!userCourses || userCourses.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="5" class="text-center">Bạn chưa đăng ký khóa học nào</td>
                </tr>
            `);
            return;
        }

        userCourses.forEach(userCourse => {
            const course = userCourse.Course;
            if (!course) return;

            const isCompleted = userCourse.CompletedAt != null;
            const progress = userCourse.Progress || 0;

            const userCourseActions = generateUserCourseActions(userCourse);

            const row = `
                <tr>
                    <td>
                        <strong>${escapeHtml(course.Title)}</strong>
                        ${course.Description ? `<br><small class="text-muted">${escapeHtml(course.Description.substring(0, 80))}...</small>` : ''}
                    </td>
                    <td>${formatDate(userCourse.RegisteredAt)}</td>
                    <td>
                        <span class="badge bg-${isCompleted ? 'success' : 'primary'}">
                            ${isCompleted ? 'Hoàn thành' : 'Đang học'}
                        </span>
                    </td>
                    <td>
                        <div class="progress">
                            <div class="progress-bar ${isCompleted ? 'bg-success' : 'bg-primary'}" 
                                 style="width: ${progress}%" 
                                 title="${progress}% hoàn thành">
                                ${progress}%
                            </div>
                        </div>
                    </td>
                    <td>${userCourseActions}</td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Generate user course action buttons
    function generateUserCourseActions(userCourse) {
        let buttons = '';

        // View course details
        buttons += `<button type="button" class="btn btn-info btn-sm me-1" onclick="viewCourse(${userCourse.Course.CourseId})" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button>`;

        // Mark as completed (if not completed)
        if (!userCourse.CompletedAt) {
            buttons += `<button type="button" class="btn btn-success btn-sm me-1" onclick="markAsCompleted(${userCourse.UserCourseId})" title="Đánh dấu hoàn thành">
                            <i class="fas fa-check"></i>
                        </button>`;
        }

        // Unregister (only for members, not staff+)
        if (USER_PERMISSIONS.userRole === 'Member') {
            buttons += `<button type="button" class="btn btn-danger btn-sm" onclick="unregisterCourse(${userCourse.UserCourseId})" title="Hủy đăng ký">
                            <i class="fas fa-times"></i>
                        </button>`;
        }

        return `<div class="btn-group btn-group-sm" role="group">${buttons}</div>`;
    }

    // Course action functions
    window.viewCourse = function (courseId) {
        window.location.href = `/Courses/Details/${courseId}`;
    };

    window.registerForCourse = function (courseId) {
        if (!USER_PERMISSIONS.isAuthenticated) {
            showAlert('warning', 'Bạn cần đăng nhập để đăng ký khóa học');
            setTimeout(() => {
                window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            }, 2000);
            return;
        }

        if (!USER_PERMISSIONS.canRegisterCourses) {
            showAlert('error', 'Bạn không có quyền đăng ký khóa học');
            return;
        }

        // Implement course registration logic
        registerCourseAction(courseId);
    };

    window.editCourse = function (courseId) {
        if (!USER_PERMISSIONS.canManageCourses) {
            showAlert('error', 'Bạn không có quyền chỉnh sửa khóa học');
            return;
        }
        window.location.href = `/Courses/Edit/${courseId}`;
    };

    window.deleteCourse = function (courseId) {
        if (!USER_PERMISSIONS.canManageCourses) {
            showAlert('error', 'Bạn không có quyền xóa khóa học');
            return;
        }

        if (confirm('Bạn có chắc chắn muốn xóa khóa học này?')) {
            deleteCourseAction(courseId);
        }
    };

    window.markAsCompleted = function (userCourseId) {
        markAsCompletedAction(userCourseId);
    };

    window.unregisterCourse = function (userCourseId) {
        if (confirm('Bạn có chắc chắn muốn hủy đăng ký khóa học này?')) {
            unregisterCourseAction(userCourseId);
        }
    };

    // Action implementations
    function registerCourseAction(courseId) {
        const data = {
            UserId: CURRENT_USER_ID,
            CourseId: courseId,
            RegisteredAt: new Date().toISOString()
        };

        $.ajax({
            url: `${API_BASE_URL}/UserCourses`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(data),
            success: function (response) {
                showAlert('success', 'Đăng ký khóa học thành công!');
                loadUserCourses(); // Reload user courses
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi đăng ký khóa học');
            }
        });
    }

    function deleteCourseAction(courseId) {
        $.ajax({
            url: `${API_BASE_URL}/Courses(${courseId})`,
            method: 'DELETE',
            headers: setupAjaxHeaders(),
            success: function (response) {
                showAlert('success', 'Xóa khóa học thành công!');
                loadCourses(); // Reload courses
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi xóa khóa học');
            }
        });
    }

    function markAsCompletedAction(userCourseId) {
        const data = {
            CompletedAt: new Date().toISOString(),
            Progress: 100
        };

        $.ajax({
            url: `${API_BASE_URL}/UserCourses(${userCourseId})`,
            method: 'PATCH',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(data),
            success: function (response) {
                showAlert('success', 'Đánh dấu hoàn thành thành công!');
                loadUserCourses(); // Reload user courses
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi cập nhật trạng thái');
            }
        });
    }

    function unregisterCourseAction(userCourseId) {
        $.ajax({
            url: `${API_BASE_URL}/UserCourses(${userCourseId})`,
            method: 'DELETE',
            headers: setupAjaxHeaders(),
            success: function (response) {
                showAlert('success', 'Hủy đăng ký khóa học thành công!');
                loadUserCourses(); // Reload user courses
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi hủy đăng ký');
            }
        });
    }

    // Helper functions
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

    function getTargetAudienceText(audience) {
        const audiences = {
            'Student': 'Học sinh',
            'Parent': 'Phụ huynh',
            'Teacher': 'Giáo viên',
            'General': 'Cộng đồng'
        };
        return audiences[audience] || audience;
    }

    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function showLoading() {
        $('#loadingSpinner').show();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
    }

    function showAlert(type, message) {
        // Implement your alert function here
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
        // Implement pagination logic here
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
        loadCourses();
    };
});