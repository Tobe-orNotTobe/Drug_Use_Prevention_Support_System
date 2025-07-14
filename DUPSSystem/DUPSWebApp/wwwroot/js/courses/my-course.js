class MyCourse {
    constructor() {
        this.apiBaseUrl = this.getApiBaseUrl();
        this.userCoursesApiUrl = this.apiBaseUrl + '/odata/UserCourses';
        this.coursesApiUrl = this.apiBaseUrl + '/odata/Courses';
        this.currentPage = 1;
        this.pageSize = 9;
        this.totalPages = 0;
        this.currentUserCourseId = null;
        this.authToken = this.getAuthToken();
        this.currentUserId = this.getCurrentUserId();
        this.stats = {};

        this.init();
    }

    getApiBaseUrl() {
        const apiUrl = document.querySelector('meta[name="api-base-url"]');
        return apiUrl ? apiUrl.getAttribute('content') : 'https://localhost:7008';
    }

    getAuthToken() {
        const tokenElement = document.querySelector('meta[name="auth-token"]');
        return tokenElement ? tokenElement.getAttribute('content') || '' : '';
    }

    getCurrentUserId() {
        const userIdElement = document.querySelector('meta[name="current-user-id"]');
        return userIdElement ? parseInt(userIdElement.getAttribute('content')) || 0 : 0;
    }

    init() {
        this.bindEvents();
        this.loadStats();
        this.loadMyCourses();
    }

    bindEvents() {
        const btnSearch = document.getElementById('btnSearch');
        const searchCourseName = document.getElementById('searchCourseName');
        const filterStatus = document.getElementById('filterStatus');
        const filterCourseStatus = document.getElementById('filterCourseStatus');
        const btnClearFilter = document.getElementById('btnClearFilter');
        const btnRefresh = document.getElementById('btnRefresh');
        const btnStartCourse = document.getElementById('btnStartCourse');
        const btnMarkCompleted = document.getElementById('btnMarkCompleted');
        const btnUpdateProgress = document.getElementById('btnUpdateProgress');

        if (btnSearch) btnSearch.addEventListener('click', () => this.performSearch());
        if (searchCourseName) {
            searchCourseName.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') this.performSearch();
            });
        }
        if (filterStatus) filterStatus.addEventListener('change', () => this.performSearch());
        if (filterCourseStatus) filterCourseStatus.addEventListener('change', () => this.performSearch());
        if (btnClearFilter) btnClearFilter.addEventListener('click', () => this.clearFilters());
        if (btnRefresh) btnRefresh.addEventListener('click', () => this.refreshData());
        if (btnStartCourse) btnStartCourse.addEventListener('click', () => this.startCourse());
        if (btnMarkCompleted) btnMarkCompleted.addEventListener('click', () => this.markCompleted());
        if (btnUpdateProgress) btnUpdateProgress.addEventListener('click', () => this.updateProgress());
    }

    performSearch() {
        this.currentPage = 1;
        this.loadMyCourses();
    }

    clearFilters() {
        const searchCourseName = document.getElementById('searchCourseName');
        const filterStatus = document.getElementById('filterStatus');
        const filterCourseStatus = document.getElementById('filterCourseStatus');

        if (searchCourseName) searchCourseName.value = '';
        if (filterStatus) filterStatus.value = '';
        if (filterCourseStatus) filterCourseStatus.value = '';

        this.performSearch();
    }

    refreshData() {
        this.loadStats();
        this.loadMyCourses();
        this.showAlert('Đã làm mới dữ liệu', 'info');
    }

    buildApiUrl() {
        let filterParts = [];

        // filter by UserId (always required)
        filterParts.push(`UserId eq ${this.currentUserId}`);

        const searchCourseName = document.getElementById('searchCourseName');
        if (searchCourseName && searchCourseName.value.trim()) {
            filterParts.push(`contains(tolower(Course/Title), '${searchCourseName.value.toLowerCase()}')`);
        }

        const filterStatus = document.getElementById('filterStatus');
        if (filterStatus && filterStatus.value) {
            filterParts.push(`CompletionStatus eq '${filterStatus.value}'`);
        }

        const filterCourseStatus = document.getElementById('filterCourseStatus');
        if (filterCourseStatus && filterCourseStatus.value) {
            filterParts.push(`Course/IsActive eq ${filterCourseStatus.value}`);
        }

        let url = this.userCoursesApiUrl + '?$expand=Course';

        if (filterParts.length > 0) {
            url += '&$filter=' + filterParts.join(' and ');
        }

        url += '&$skip=' + ((this.currentPage - 1) * this.pageSize);
        url += '&$top=' + this.pageSize;
        url += '&$count=true';

        return url;
    }

    async loadStats() {
        try {
            const response = await fetch(this.userCoursesApiUrl + '?$expand=Course&$filter=UserId eq ' + this.currentUserId, {
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Không thể tải thống kê');
            }

            const data = await response.json();
            const courses = data.value || [];

            const stats = {
                totalRegistered: courses.length,
                totalCompleted: courses.filter(c => c.CompletionStatus === 'Completed').length,
                totalInProgress: courses.filter(c => c.CompletionStatus === 'InProgress').length,
                totalNotStarted: courses.filter(c => c.CompletionStatus === 'NotStarted').length
            };

            this.updateStats(stats);

        } catch (error) {
            console.error('Error loading stats:', error);
        }
    }

    updateStats(stats) {
        this.stats = stats;

        const totalRegisteredEl = document.getElementById('totalRegistered');
        const totalCompletedEl = document.getElementById('totalCompleted');
        const totalInProgressEl = document.getElementById('totalInProgress');
        const totalNotStartedEl = document.getElementById('totalNotStarted');

        if (totalRegisteredEl) totalRegisteredEl.textContent = stats.totalRegistered;
        if (totalCompletedEl) totalCompletedEl.textContent = stats.totalCompleted;
        if (totalInProgressEl) totalInProgressEl.textContent = stats.totalInProgress;
        if (totalNotStartedEl) totalNotStartedEl.textContent = stats.totalNotStarted;
    }

    async loadMyCourses() {
        try {
            this.showLoading();

            const response = await fetch(this.buildApiUrl(), {
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('HTTP error! status: ' + response.status);
            }

            const data = await response.json();

            this.renderCourses(data.value || []);
            this.renderPagination(data['@odata.count'] || 0);

        } catch (error) {
            console.error('Error loading my courses:', error);
            this.showAlert('Có lỗi xảy ra khi tải danh sách khóa học của bạn', 'danger');
            this.showEmptyState();
        }
    }

    showLoading() {
        const container = document.getElementById('courseCardsContainer');
        if (container) {
            container.innerHTML = '<div class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Đang tải...</span></div></div>';
        }
    }

    renderCourses(userCourses) {
        const container = document.getElementById('courseCardsContainer');
        if (!container) return;

        if (userCourses.length === 0) {
            this.showEmptyState();
            return;
        }

        const cards = userCourses.map(userCourse => this.createCourseCard(userCourse)).join('');
        container.innerHTML = '<div class="row">' + cards + '</div>';
    }

    createCourseCard(userCourse) {
        const course = userCourse.Course;
        if (!course) return '';

        const statusInfo = this.getStatusInfo(userCourse.CompletionStatus);
        const courseStatusBadge = course.IsActive
            ? '<span class="badge bg-success">Hoạt động</span>'
            : '<span class="badge bg-secondary">Không hoạt động</span>';

        const registrationDate = userCourse.RegisteredAt
            ? new Date(userCourse.RegisteredAt).toLocaleDateString('vi-VN')
            : '';

        const completionDate = userCourse.CompletedAt
            ? new Date(userCourse.CompletedAt).toLocaleDateString('vi-VN')
            : '';

        const durationInHours = course.DurationMinutes ? Math.round(course.DurationMinutes / 60 * 10) / 10 : 0;

        const progressPercentage = this.calculateProgress(userCourse.CompletionStatus);

        return '<div class="col-md-4">' +
            '<div class="card course-card mb-4">' +
            '<div class="card-body position-relative">' +
            '<span class="badge ' + statusInfo.class + ' status-badge">' + statusInfo.text + '</span>' +
            '<h5 class="card-title">' + (course.Title || '') + '</h5>' +
            '<p class="card-text text-muted">' +
            (course.Description ? (course.Description.length > 100 ? course.Description.substring(0, 100) + '...' : course.Description) : '') +
            '</p>' +
            '<div class="course-meta mb-3">' +
            '<div class="row">' +
            '<div class="col-6">' +
            '<i class="fas fa-users"></i> ' + (course.TargetAudience || '') +
            '</div>' +
            '<div class="col-6">' +
            '<i class="fas fa-clock"></i> ' + durationInHours + ' giờ' +
            '</div>' +
            '</div>' +
            '<div class="row mt-2">' +
            '<div class="col-6">' +
            '<i class="fas fa-calendar"></i> ' + registrationDate +
            '</div>' +
            '<div class="col-6">' +
            courseStatusBadge +
            '</div>' +
            '</div>' +
            (completionDate ? '<div class="row mt-2"><div class="col-12"><i class="fas fa-check-circle text-success"></i> Hoàn thành: ' + completionDate + '</div></div>' : '') +
            '</div>' +
            '<div class="progress-bar-container mb-3">' +
            '<div class="progress-bar-fill bg-' + statusInfo.progressColor + '" style="width: ' + progressPercentage + '%"></div>' +
            '</div>' +
            '<div class="d-flex gap-2">' +
            '<button class="btn btn-sm btn-outline-primary flex-fill" onclick="myCourse.viewCourseDetail(' + userCourse.UserCourseId + ')">' +
            '<i class="fas fa-eye"></i> Chi tiết' +
            '</button>' +
            '<button class="btn btn-sm btn-outline-warning" onclick="myCourse.showProgressModal(' + userCourse.UserCourseId + ', \'' + userCourse.CompletionStatus + '\')">' +
            '<i class="fas fa-edit"></i> Cập nhật' +
            '</button>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
    }

    getStatusInfo(status) {
        switch (status) {
            case 'Completed':
                return { text: 'Đã hoàn thành', class: 'bg-success', progressColor: 'success' };
            case 'InProgress':
                return { text: 'Đang học', class: 'bg-warning', progressColor: 'warning' };
            case 'NotStarted':
            default:
                return { text: 'Chưa bắt đầu', class: 'bg-secondary', progressColor: 'secondary' };
        }
    }

    calculateProgress(status) {
        switch (status) {
            case 'Completed': return 100;
            case 'InProgress': return 50;
            case 'NotStarted':
            default: return 0;
        }
    }

    showEmptyState() {
        const container = document.getElementById('courseCardsContainer');
        if (container) {
            container.innerHTML =
                '<div class="empty-state">' +
                '<i class="fas fa-book-open"></i>' +
                '<h4 class="text-muted">Bạn chưa đăng ký khóa học nào</h4>' +
                '<p class="text-muted">Hãy tìm và đăng ký các khóa học phù hợp với bạn</p>' +
                '<a href="/Courses/Index" class="btn btn-primary">' +
                '<i class="fas fa-search"></i> Tìm khóa học' +
                '</a>' +
                '</div>';
        }
    }

    renderPagination(totalCount) {
        this.totalPages = Math.ceil(totalCount / this.pageSize);
        const pagination = document.getElementById('pagination');
        if (!pagination) return;

        if (this.totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let paginationHtml = '';

        if (this.currentPage > 1) {
            paginationHtml += '<li class="page-item"><a class="page-link" href="#" onclick="myCourse.goToPage(' + (this.currentPage - 1) + ')">Trước</a></li>';
        }

        for (let i = Math.max(1, this.currentPage - 2); i <= Math.min(this.totalPages, this.currentPage + 2); i++) {
            const activeClass = i === this.currentPage ? 'active' : '';
            paginationHtml += '<li class="page-item ' + activeClass + '"><a class="page-link" href="#" onclick="myCourse.goToPage(' + i + ')">' + i + '</a></li>';
        }

        if (this.currentPage < this.totalPages) {
            paginationHtml += '<li class="page-item"><a class="page-link" href="#" onclick="myCourse.goToPage(' + (this.currentPage + 1) + ')">Sau</a></li>';
        }

        pagination.innerHTML = paginationHtml;
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadMyCourses();
    }

    async viewCourseDetail(userCourseId) {
        try {
            const response = await fetch(this.userCoursesApiUrl + '(' + userCourseId + ')?$expand=Course', {
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Không thể tải thông tin khóa học');
            }

            const userCourse = await response.json();
            this.showCourseDetailModal(userCourse);

        } catch (error) {
            console.error('Error viewing course detail:', error);
            this.showAlert('Có lỗi xảy ra khi tải thông tin khóa học', 'danger');
        }
    }

    showCourseDetailModal(userCourse) {
        const course = userCourse.Course;
        if (!course) return;

        this.currentUserCourseId = userCourse.UserCourseId;

        const statusInfo = this.getStatusInfo(userCourse.CompletionStatus);
        const registrationDate = userCourse.RegisteredAt
            ? new Date(userCourse.RegisteredAt).toLocaleString('vi-VN')
            : '';
        const completionDate = userCourse.CompletedAt
            ? new Date(userCourse.CompletedAt).toLocaleString('vi-VN')
            : '';
        const durationInHours = course.DurationMinutes ? Math.round(course.DurationMinutes / 60 * 10) / 10 : 0;

        const modalBody = document.getElementById('courseDetailBody');
        if (!modalBody) return;

        modalBody.innerHTML =
            '<div class="row">' +
            '<div class="col-md-12">' +
            '<h4>' + (course.Title || '') + '</h4>' +
            '<span class="badge ' + statusInfo.class + ' mb-3">' + statusInfo.text + '</span>' +
            '</div>' +
            '</div>' +
            '<div class="row mb-3">' +
            '<div class="col-md-6">' +
            '<strong>Đối tượng:</strong> ' + (course.TargetAudience || '') +
            '</div>' +
            '<div class="col-md-6">' +
            '<strong>Thời lượng:</strong> ' + durationInHours + ' giờ' +
            '</div>' +
            '</div>' +
            '<div class="row mb-3">' +
            '<div class="col-md-6">' +
            '<strong>Ngày đăng ký:</strong> ' + registrationDate +
            '</div>' +
            '<div class="col-md-6">' +
            '<strong>Trạng thái khóa học:</strong> ' + (course.IsActive ? 'Hoạt động' : 'Không hoạt động') +
            '</div>' +
            '</div>' +
            (completionDate ? '<div class="row mb-3"><div class="col-md-12"><strong>Ngày hoàn thành:</strong> ' + completionDate + '</div></div>' : '') +
            (course.Description ? '<div class="row"><div class="col-md-12"><strong>Mô tả:</strong><div class="mt-2 p-3 bg-light rounded">' + course.Description + '</div></div></div>' : '');

        const btnStartCourse = document.getElementById('btnStartCourse');
        const btnMarkCompleted = document.getElementById('btnMarkCompleted');

        if (btnStartCourse) {
            btnStartCourse.style.display = userCourse.CompletionStatus === 'NotStarted' ? 'inline-block' : 'none';
        }

        if (btnMarkCompleted) {
            btnMarkCompleted.style.display = userCourse.CompletionStatus === 'InProgress' ? 'inline-block' : 'none';
        }

        const modal = new bootstrap.Modal(document.getElementById('courseDetailModal'));
        modal.show();
    }

    showProgressModal(userCourseId, currentStatus) {
        this.currentUserCourseId = userCourseId;

        const statusNotStarted = document.getElementById('statusNotStarted');
        const statusInProgress = document.getElementById('statusInProgress');
        const statusCompleted = document.getElementById('statusCompleted');

        if (statusNotStarted) statusNotStarted.checked = currentStatus === 'NotStarted';
        if (statusInProgress) statusInProgress.checked = currentStatus === 'InProgress';
        if (statusCompleted) statusCompleted.checked = currentStatus === 'Completed';

        const modal = new bootstrap.Modal(document.getElementById('progressModal'));
        modal.show();
    }

    async startCourse() {
        await this.updateCourseStatus('InProgress');
    }

    async markCompleted() {
        await this.updateCourseStatus('Completed');
    }

    async updateProgress() {
        const checkedRadio = document.querySelector('input[name="progressStatus"]:checked');
        if (!checkedRadio) {
            this.showAlert('Vui lòng chọn trạng thái', 'warning');
            return;
        }

        await this.updateCourseStatus(checkedRadio.value);

        const modal = bootstrap.Modal.getInstance(document.getElementById('progressModal'));
        if (modal) modal.hide();
    }

    async updateCourseStatus(newStatus) {
        if (!this.currentUserCourseId) return;

        try {
            const updateData = {
                CompletionStatus: newStatus
            };

            if (newStatus === 'Completed') {
                updateData.CompletedAt = new Date().toISOString();
            }

            const response = await fetch(this.userCoursesApiUrl + '(' + this.currentUserCourseId + ')', {
                method: 'PATCH',
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updateData)
            });

            if (!response.ok) {
                throw new Error('Không thể cập nhật trạng thái');
            }

            const statusText = this.getStatusInfo(newStatus).text;
            this.showAlert('Đã cập nhật trạng thái thành: ' + statusText, 'success');

            this.loadStats();
            this.loadMyCourses();

            const detailModal = bootstrap.Modal.getInstance(document.getElementById('courseDetailModal'));
            if (detailModal) detailModal.hide();

        } catch (error) {
            console.error('Error updating course status:', error);
            this.showAlert('Có lỗi xảy ra khi cập nhật trạng thái', 'danger');
        }
    }

    showAlert(message, type) {
        const alertContainer = document.getElementById('alertContainer');
        if (!alertContainer) return;

        const alertId = 'alert-' + Date.now();
        const iconClass = type === 'success' ? 'check-circle' : (type === 'danger' ? 'exclamation-triangle' : 'info-circle');

        const alertHtml =
            '<div id="' + alertId + '" class="alert alert-' + type + ' alert-dismissible fade show" role="alert">' +
            '<i class="fas fa-' + iconClass + '"></i> ' + message +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>' +
            '</div>';

        alertContainer.insertAdjacentHTML('beforeend', alertHtml);

        setTimeout(function () {
            const alertElement = document.getElementById(alertId);
            if (alertElement) {
                alertElement.remove();
            }
        }, 5000);
    }
}

var myCourse = new MyCourse();