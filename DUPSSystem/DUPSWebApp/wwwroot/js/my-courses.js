// my-courses.js - Fixed version
$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata'; // Fixed double slash
    let CURRENT_USER_ID = window.CURRENT_USER_ID || null;

    let userCourses = [];
    let filteredCourses = [];

    // Check if user is logged in
    if (!CURRENT_USER_ID || !window.USER_TOKEN) {
        showAlert('warning', 'Bạn cần đăng nhập để xem khóa học của mình');
        setTimeout(() => {
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }, 2000);
        return;
    }

    // Initialize page
    init();

    function init() {
        loadUserCourses();
        bindEvents();
    }

    function bindEvents() {
        // Filter and search functionality
        $('#statusFilter, #sortFilter').change(function () {
            filterAndDisplayCourses();
        });

        $('#searchInput').on('input', function () {
            filterAndDisplayCourses();
        });

        // Modal events
        $('#markCompletedBtn').click(function () {
            const courseId = $(this).data('course-id');
            markAsCompleted(CURRENT_USER_ID, courseId);
        });

        $('#unregisterBtn').click(function () {
            const userCourseId = $(this).data('usercourse-id');
            unregisterCourse(userCourseId);
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        return {
            'Authorization': `Bearer ${window.USER_TOKEN}`,
            'Content-Type': 'application/json'
        };
    }

    // Load user's registered courses
    function loadUserCourses() {
        showLoading();

        const odataQuery = `${API_BASE_URL}/UserCourses?$filter=UserId eq ${CURRENT_USER_ID}&$expand=Course&$orderby=RegisteredAt desc`;

        console.log('Loading user courses:', odataQuery); // Debug log

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                hideLoading();
                console.log('API Response:', response); // Debug log

                userCourses = response.value || response || [];
                filteredCourses = [...userCourses];

                console.log('User courses loaded:', userCourses.length); // Debug log

                displayCourses();
                updateStatistics();
            },
            error: function (xhr, status, error) {
                hideLoading();
                console.error('Error loading user courses:', error);
                console.error('XHR:', xhr); // Debug log

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 2000);
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi tải danh sách khóa học: ' + (xhr.responseJSON?.message || error));
                }
            }
        });
    }

    // Filter and display courses
    function filterAndDisplayCourses() {
        const statusFilter = $('#statusFilter').val();
        const sortFilter = $('#sortFilter').val();
        const searchTerm = $('#searchInput').val().toLowerCase().trim();

        // Apply filters
        filteredCourses = userCourses.filter(userCourse => {
            const course = userCourse.Course;
            if (!course) return false; // Skip if course is null

            // Status filter
            if (statusFilter === 'active' && userCourse.CompletedAt) return false;
            if (statusFilter === 'completed' && !userCourse.CompletedAt) return false;

            // Search filter
            if (searchTerm && !course.Title.toLowerCase().includes(searchTerm) &&
                (!course.Description || !course.Description.toLowerCase().includes(searchTerm))) {
                return false;
            }

            return true;
        });

        // Apply sorting
        switch (sortFilter) {
            case 'newest':
                filteredCourses.sort((a, b) => new Date(b.RegisteredAt) - new Date(a.RegisteredAt));
                break;
            case 'oldest':
                filteredCourses.sort((a, b) => new Date(a.RegisteredAt) - new Date(b.RegisteredAt));
                break;
            case 'completed':
                filteredCourses.sort((a, b) => {
                    if (a.CompletedAt && b.CompletedAt) {
                        return new Date(b.CompletedAt) - new Date(a.CompletedAt);
                    }
                    if (a.CompletedAt && !b.CompletedAt) return -1;
                    if (!a.CompletedAt && b.CompletedAt) return 1;
                    return 0;
                });
                break;
        }

        displayCourses();
    }

    // Display courses in grid
    function displayCourses() {
        const coursesGrid = $('#coursesGrid');
        const emptyState = $('#emptyState');

        coursesGrid.empty();

        if (filteredCourses.length === 0) {
            coursesGrid.hide();
            emptyState.show();
            return;
        }

        emptyState.hide();
        coursesGrid.show();

        filteredCourses.forEach(userCourse => {
            const course = userCourse.Course;
            if (!course) return; // Skip if course is null

            const isCompleted = userCourse.CompletedAt != null;
            const progressPercentage = isCompleted ? 100 : Math.floor(Math.random() * 60 + 20); // Mock progress

            const courseCard = `
                <div class="col-lg-4 col-md-6 mb-4">
                    <div class="card h-100 course-card" data-course-id="${course.CourseId}">
                        <div class="card-header bg-${isCompleted ? 'success' : 'primary'} text-white">
                            <h5 class="card-title mb-0">${escapeHtml(course.Title)}</h5>
                            <span class="badge bg-light text-dark float-end">
                                ${getTargetAudienceText(course.TargetAudience)}
                            </span>
                        </div>
                        <div class="card-body">
                            <p class="card-text">${course.Description ? escapeHtml(course.Description.substring(0, 100)) + (course.Description.length > 100 ? '...' : '') : 'Không có mô tả'}</p>
                            
                            <div class="mb-2">
                                <small class="text-muted">Tiến độ:</small>
                                <div class="progress">
                                    <div class="progress-bar ${isCompleted ? 'bg-success' : 'bg-primary'}" 
                                         role="progressbar" 
                                         style="width: ${progressPercentage}%"
                                         aria-valuenow="${progressPercentage}" 
                                         aria-valuemin="0" 
                                         aria-valuemax="100">
                                        ${progressPercentage}%
                                    </div>
                                </div>
                            </div>

                            <div class="row text-center">
                                <div class="col-6">
                                    <small class="text-muted">Đăng ký:</small><br>
                                    <strong>${formatDate(userCourse.RegisteredAt)}</strong>
                                </div>
                                <div class="col-6">
                                    <small class="text-muted">Thời lượng:</small><br>
                                    <strong>${course.DurationMinutes ? course.DurationMinutes + ' phút' : 'N/A'}</strong>
                                </div>
                            </div>
                            
                            ${userCourse.CompletedAt ? `
                                <div class="mt-2 text-center">
                                    <small class="text-muted">Hoàn thành:</small><br>
                                    <strong class="text-success">${formatDate(userCourse.CompletedAt)}</strong>
                                </div>
                            ` : ''}
                        </div>
                        <div class="card-footer">
                            <div class="btn-group btn-group-sm w-100" role="group">
                                <button type="button" 
                                        class="btn btn-info" 
                                        onclick="viewCourseDetail(${userCourse.UserCourseId}, ${course.CourseId})"
                                        title="Xem chi tiết">
                                    <i class="fas fa-eye"></i> Chi tiết
                                </button>
                                ${!isCompleted ? `
                                    <button type="button" 
                                            class="btn btn-success" 
                                            onclick="markAsCompleted(${CURRENT_USER_ID}, ${course.CourseId})"
                                            title="Đánh dấu hoàn thành">
                                        <i class="fas fa-check"></i> Hoàn thành
                                    </button>
                                ` : `
                                    <button type="button" class="btn btn-success disabled">
                                        <i class="fas fa-check-circle"></i> Đã hoàn thành
                                    </button>
                                `}
                                <button type="button" 
                                        class="btn btn-danger" 
                                        onclick="unregisterCourse(${userCourse.UserCourseId})"
                                        title="Hủy đăng ký">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            coursesGrid.append(courseCard);
        });
    }

    // Update statistics
    function updateStatistics() {
        const totalCourses = userCourses.length;
        const completedCourses = userCourses.filter(uc => uc.CompletedAt).length;
        const activeCourses = totalCourses - completedCourses;
        const totalMinutes = userCourses.reduce((total, uc) => {
            return total + (uc.Course?.DurationMinutes || 0);
        }, 0);
        const totalHours = Math.round(totalMinutes / 60 * 10) / 10;

        $('#totalCourses').text(totalCourses);
        $('#completedCourses').text(completedCourses);
        $('#activeCourses').text(activeCourses);
        $('#totalHours').text(totalHours);
    }

    // View course detail
    function viewCourseDetail(userCourseId, courseId) {
        const userCourse = userCourses.find(uc => uc.UserCourseId === userCourseId);
        if (!userCourse || !userCourse.Course) return;

        const course = userCourse.Course;
        const isCompleted = userCourse.CompletedAt != null;

        const detailContent = `
            <div class="row">
                <div class="col-md-8">
                    <h4>${escapeHtml(course.Title)}</h4>
                    <p class="text-muted">${course.Description ? escapeHtml(course.Description) : 'Không có mô tả'}</p>
                </div>
                <div class="col-md-4">
                    <span class="badge bg-${isCompleted ? 'success' : 'primary'} fs-6 p-2">
                        ${isCompleted ? 'Đã hoàn thành' : 'Đang học'}
                    </span>
                </div>
            </div>
            
            <hr>
            
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-users"></i> Đối tượng:</h6>
                    <p>${getTargetAudienceText(course.TargetAudience)}</p>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-clock"></i> Thời lượng:</h6>
                    <p>${course.DurationMinutes ? course.DurationMinutes + ' phút' : 'Chưa xác định'}</p>
                </div>
            </div>
            
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-calendar-plus"></i> Ngày đăng ký:</h6>
                    <p>${formatDate(userCourse.RegisteredAt)}</p>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-calendar-check"></i> Ngày hoàn thành:</h6>
                    <p>${userCourse.CompletedAt ? formatDate(userCourse.CompletedAt) : 'Chưa hoàn thành'}</p>
                </div>
            </div>
            
            ${!isCompleted ? `
                <div class="alert alert-info">
                    <i class="fas fa-info-circle"></i>
                    Bạn đang học khóa học này. Hãy hoàn thành để nhận chứng chỉ!
                </div>
            ` : `
                <div class="alert alert-success">
                    <i class="fas fa-check-circle"></i>
                    Chúc mừng! Bạn đã hoàn thành khóa học này.
                </div>
            `}
        `;

        $('#courseDetailContent').html(detailContent);

        // Setup modal buttons
        $('#markCompletedBtn').data('course-id', courseId);
        $('#unregisterBtn').data('usercourse-id', userCourseId);

        if (isCompleted) {
            $('#markCompletedBtn').hide();
        } else {
            $('#markCompletedBtn').show();
        }

        new bootstrap.Modal(document.getElementById('courseDetailModal')).show();
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
                const modalInstance = bootstrap.Modal.getInstance(document.getElementById('courseDetailModal'));
                if (modalInstance) {
                    modalInstance.hide();
                }
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
                const modalInstance = bootstrap.Modal.getInstance(document.getElementById('courseDetailModal'));
                if (modalInstance) {
                    modalInstance.hide();
                }
                loadUserCourses();
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

    // Utility functions
    function showLoading() {
        $('#loadingSpinner').show();
        $('#coursesGrid').hide();
        $('#emptyState').hide();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
        $('#coursesGrid').show();
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
    window.viewCourseDetail = viewCourseDetail;
    window.markAsCompleted = markAsCompleted;
    window.unregisterCourse = unregisterCourse;
});