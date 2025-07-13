// courses.js - Updated with Authentication
$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata'; // Update to your API URL
    const CURRENT_USER_ID = window.CURRENT_USER_ID || 2; // Get from global variable or default

    let currentPage = 1;
    let pageSize = 10;
    let totalRecords = 0;

    // Check if user is logged in
    if (!window.USER_TOKEN) {
        showAlert('warning', 'Bạn cần đăng nhập để sử dụng tính năng này');
        setTimeout(() => {
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }, 2000);
        return;
    }

    // Initialize page
    init();

    function init() {
        loadCourses();
        loadUserCourses();
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

        // Add course form
        $('#addCourseForm').submit(function (e) {
            e.preventDefault();
            addCourse();
        });

        // Edit course form
        $('#editCourseForm').submit(function (e) {
            e.preventDefault();
            updateCourse();
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        return {
            'Authorization': `Bearer ${window.USER_TOKEN}`,
            'Content-Type': 'application/json'
        };
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
                // Handle direct OData response
                const courses = response.value || response;
                displayCourses(courses);
                totalRecords = response['@odata.count'] || courses.length;
                updatePagination();
            },
            error: function (xhr, status, error) {
                hideLoading();
                console.error('Error loading courses:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 2000);
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi tải danh sách khóa học');
                }
            }
        });
    }

    // Display courses in table
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
                    <td>
                        <div class="btn-group btn-group-sm" role="group">
                            <button type="button" class="btn btn-info" onclick="viewCourse(${course.CourseId})" title="Xem chi tiết">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="btn btn-warning" onclick="editCourse(${course.CourseId})" title="Chỉnh sửa">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-danger" onclick="deleteCourse(${course.CourseId})" title="Xóa">
                                <i class="fas fa-trash"></i>
                            </button>
                            <button type="button" class="btn btn-success" onclick="registerForCourse(${course.CourseId})" title="Đăng ký">
                                <i class="fas fa-user-plus"></i>
                            </button>
                        </div>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Load user's registered courses
    function loadUserCourses() {
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
                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn');
                }
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
            if (!course) return; // Skip if course is null

            const isCompleted = userCourse.CompletedAt != null;

            const row = `
                <tr>
                    <td>
                        <strong>${escapeHtml(course.Title)}</strong>
                        ${course.Description ? `<br><small class="text-muted">${escapeHtml(course.Description.substring(0, 80))}${course.Description.length > 80 ? '...' : ''}</small>` : ''}
                    </td>
                    <td>${formatDate(userCourse.RegisteredAt)}</td>
                    <td>${userCourse.CompletedAt ? formatDate(userCourse.CompletedAt) : '-'}</td>
                    <td>
                        <span class="badge bg-${isCompleted ? 'success' : 'primary'}">
                            ${isCompleted ? 'Đã hoàn thành' : 'Đang học'}
                        </span>
                    </td>
                    <td>
                        <div class="btn-group btn-group-sm" role="group">
                            ${!isCompleted ? `
                                <button type="button" class="btn btn-success" onclick="markAsCompleted(${userCourse.UserId}, ${userCourse.CourseId})" title="Đánh dấu hoàn thành">
                                    <i class="fas fa-check"></i> Hoàn thành
                                </button>
                            ` : ''}
                            <button type="button" class="btn btn-danger" onclick="unregisterCourse(${userCourse.UserCourseId})" title="Hủy đăng ký">
                                <i class="fas fa-times"></i> Hủy
                            </button>
                        </div>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Add new course
    function addCourse() {
        const courseData = {
            Title: $('#courseTitle').val().trim(),
            Description: $('#courseDescription').val().trim() || null,
            TargetAudience: $('#targetAudience').val() || null,
            DurationMinutes: parseInt($('#durationMinutes').val()) || null,
            IsActive: $('#isActive').is(':checked')
        };

        if (!courseData.Title) {
            showAlert('warning', 'Vui lòng nhập tiêu đề khóa học');
            return;
        }

        $.ajax({
            url: `${API_BASE_URL}/Courses`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(courseData),
            success: function (response) {
                showAlert('success', 'Thêm khóa học thành công');
                bootstrap.Modal.getInstance(document.getElementById('addCourseModal')).hide();
                $('#addCourseForm')[0].reset();
                loadCourses();
            },
            error: function (xhr, status, error) {
                console.error('Error adding course:', error);
                if (xhr.status === 401) {
                    showAlert('error', 'Bạn không có quyền thực hiện hành động này');
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi thêm khóa học');
                }
            }
        });
    }

    // Register for course
    function registerForCourse(courseId) {
        const registrationData = {
            UserId: CURRENT_USER_ID,
            CourseId: courseId
        };

        $.ajax({
            url: `${API_BASE_URL}/UserCourses`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(registrationData),
            success: function (response) {
                showAlert('success', 'Đăng ký khóa học thành công');
                loadUserCourses();
            },
            error: function (xhr, status, error) {
                console.error('Error registering for course:', error);
                if (xhr.status === 401) {
                    showAlert('error', 'Bạn cần đăng nhập để đăng ký khóa học');
                } else {
                    const errorMessage = xhr.responseJSON?.message || 'Đã xảy ra lỗi khi đăng ký khóa học';
                    showAlert('error', errorMessage);
                }
            }
        });
    }

    // View course details (keeping same logic as before)
    function viewCourse(courseId) {
        $.ajax({
            url: `${API_BASE_URL}/Courses(${courseId})`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const course = response;
                showCourseModal(course);
            },
            error: function (xhr, status, error) {
                console.error('Error viewing course:', error);
                showAlert('error', 'Đã xảy ra lỗi khi tải thông tin khóa học');
            }
        });
    }

    function showCourseModal(course) {
        const modalContent = `
            <div class="modal fade" id="viewCourseModal" tabindex="-1" aria-labelledby="viewCourseModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="viewCourseModalLabel">Chi tiết khóa học</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <strong>Tiêu đề:</strong><br>
                                    <p>${escapeHtml(course.Title)}</p>
                                </div>
                                <div class="col-md-6">
                                    <strong>Trạng thái:</strong><br>
                                    <span class="badge bg-${course.IsActive ? 'success' : 'secondary'}">
                                        ${course.IsActive ? 'Hoạt động' : 'Không hoạt động'}
                                    </span>
                                </div>
                            </div>
                            <div class="row mt-3">
                                <div class="col-12">
                                    <strong>Mô tả:</strong><br>
                                    <p>${course.Description ? escapeHtml(course.Description) : 'Không có mô tả'}</p>
                                </div>
                            </div>
                            <div class="row mt-3">
                                <div class="col-md-6">
                                    <strong>Ngày tạo:</strong><br>
                                    <p>${formatDate(course.CreatedAt)}</p>
                                </div>
                                <div class="col-md-6">
                                    <strong>Cập nhật lần cuối:</strong><br>
                                    <p>${course.UpdatedAt ? formatDate(course.UpdatedAt) : 'Chưa cập nhật'}</p>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                            <button type="button" class="btn btn-success" onclick="registerForCourse(${course.CourseId})">
                                <i class="fas fa-user-plus"></i> Đăng ký
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remove existing modal if any
        $('#viewCourseModal').remove();

        // Add and show new modal
        $('body').append(modalContent);
        new bootstrap.Modal(document.getElementById('viewCourseModal')).show();
    }

    // Edit course
    function editCourse(courseId) {
        $.ajax({
            url: `${API_BASE_URL}/Courses(${courseId})`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const course = response;

                // Populate edit form
                $('#editCourseId').val(course.CourseId);
                $('#editCourseTitle').val(course.Title);
                $('#editCourseDescription').val(course.Description || '');
                $('#editTargetAudience').val(course.TargetAudience || '');
                $('#editDurationMinutes').val(course.DurationMinutes || '');
                $('#editIsActive').prop('checked', course.IsActive);

                // Show modal
                new bootstrap.Modal(document.getElementById('editCourseModal')).show();
            },
            error: function (xhr, status, error) {
                console.error('Error loading course for edit:', error);
                if (xhr.status === 401) {
                    showAlert('error', 'Bạn không có quyền chỉnh sửa khóa học');
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi tải thông tin khóa học');
                }
            }
        });
    }

    // Update course
    function updateCourse() {
        const courseId = $('#editCourseId').val();
        const courseData = {
            Title: $('#editCourseTitle').val().trim(),
            Description: $('#editCourseDescription').val().trim() || null,
            TargetAudience: $('#editTargetAudience').val() || null,
            DurationMinutes: parseInt($('#editDurationMinutes').val()) || null,
            IsActive: $('#editIsActive').is(':checked')
        };

        if (!courseData.Title) {
            showAlert('warning', 'Vui lòng nhập tiêu đề khóa học');
            return;
        }

        $.ajax({
            url: `${API_BASE_URL}/Courses(${courseId})`,
            method: 'PATCH',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(courseData),
            success: function (response) {
                showAlert('success', 'Cập nhật khóa học thành công');
                bootstrap.Modal.getInstance(document.getElementById('editCourseModal')).hide();
                loadCourses();
            },
            error: function (xhr, status, error) {
                console.error('Error updating course:', error);
                if (xhr.status === 401) {
                    showAlert('error', 'Bạn không có quyền cập nhật khóa học');
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi cập nhật khóa học');
                }
            }
        });
    }

    // Delete course
    function deleteCourse(courseId) {
        if (!confirm('Bạn có chắc chắn muốn xóa khóa học này?')) {
            return;
        }

        $.ajax({
            url: `${API_BASE_URL}/Courses(${courseId})`,
            method: 'DELETE',
            headers: setupAjaxHeaders(),
            success: function (response) {
                showAlert('success', 'Xóa khóa học thành công');
                loadCourses();
            },
            error: function (xhr, status, error) {
                console.error('Error deleting course:', error);
                if (xhr.status === 401) {
                    showAlert('error', 'Bạn không có quyền xóa khóa học');
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi xóa khóa học');
                }
            }
        });
    }

    // Mark course as completed
    function markAsCompleted(userId, courseId) {
        const data = {
            UserId: userId,
            CourseId: courseId
        };

        $.ajax({
            url: `${API_BASE_URL}/UserCourses/Complete`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(data),
            success: function (response) {
                showAlert('success', 'Đánh dấu hoàn thành khóa học thành công');
                loadUserCourses();
            },
            error: function (xhr, status, error) {
                console.error('Error marking course as completed:', error);
                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn');
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi đánh dấu hoàn thành khóa học');
                }
            }
        });
    }

    // Unregister from course
    function unregisterCourse(userCourseId) {
        if (!confirm('Bạn có chắc chắn muốn hủy đăng ký khóa học này?')) {
            return;
        }

        $.ajax({
            url: `${API_BASE_URL}/UserCourses(${userCourseId})`,
            method: 'DELETE',
            headers: setupAjaxHeaders(),
            success: function (response) {
                showAlert('success', 'Hủy đăng ký khóa học thành công');
                loadUserCourses();
                loadCourses(); // Refresh to update registration status
            },
            error: function (xhr, status, error) {
                console.error('Error unregistering course:', error);
                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn');
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi hủy đăng ký khóa học');
                }
            }
        });
    }

    // Update pagination
    function updatePagination() {
        const totalPages = Math.ceil(totalRecords / pageSize);
        const pagination = $('#pagination');
        pagination.empty();

        if (totalPages <= 1) return;

        // Previous button
        pagination.append(`
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="changePage(${currentPage - 1})">Trước</a>
            </li>
        `);

        // Page numbers
        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(totalPages, currentPage + 2);

        for (let i = startPage; i <= endPage; i++) {
            pagination.append(`
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
                </li>
            `);
        }

        // Next button
        pagination.append(`
            <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="changePage(${currentPage + 1})">Sau</a>
            </li>
        `);
    }

    // Change page
    window.changePage = function (page) {
        if (page >= 1 && page <= Math.ceil(totalRecords / pageSize)) {
            currentPage = page;
            loadCourses();
        }
    };

    // Utility functions
    function showLoading() {
        $('#loadingSpinner').show();
        $('#coursesTableBody').hide();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
        $('#coursesTableBody').show();
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
        return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    }

    function getTargetAudienceText(audience) {
        const audiences = {
            'students': 'Học sinh',
            'parents': 'Phụ huynh',
            'teachers': 'Giáo viên',
            'general': 'Cộng đồng'
        };
        return audiences[audience] || audience || 'Không xác định';
    }

    // Make functions global for onclick handlers
    window.viewCourse = viewCourse;
    window.editCourse = editCourse;
    window.deleteCourse = deleteCourse;
    window.registerForCourse = registerForCourse;
    window.markAsCompleted = markAsCompleted;
    window.unregisterCourse = unregisterCourse;
});