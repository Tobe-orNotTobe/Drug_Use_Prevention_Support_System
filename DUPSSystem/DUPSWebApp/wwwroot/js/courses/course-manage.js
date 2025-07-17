class CourseManage {
    constructor() {
        this.apiBaseUrl = this.getApiBaseUrl() + '/odata/Courses';
        this.userCoursesApiUrl = this.getApiBaseUrl() + '/odata/UserCourses';
        this.currentPage = 1;
        this.pageSize = 15;
        this.totalPages = 0;
        this.currentCourseId = null;
        this.authToken = this.getAuthToken();
        this.stats = {};
        this.newStatus = null;

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

    init() {
        this.bindEvents();
        this.loadDashboardStats();
        this.loadCourses();
    }

    bindEvents() {
        const btnSearch = document.getElementById('btnSearch');
        const searchName = document.getElementById('searchName');
        const searchTargetAudience = document.getElementById('searchTargetAudience');
        const searchStatus = document.getElementById('searchStatus');
        const btnClearSearch = document.getElementById('btnClearSearch');
        const btnRefresh = document.getElementById('btnRefresh');
        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
        const confirmStatusBtn = document.getElementById('confirmStatusBtn');

        if (btnSearch) {
            btnSearch.addEventListener('click', () => this.performSearch());
        }
        if (searchName) {
            searchName.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') this.performSearch();
            });
        }
        if (searchTargetAudience) {
            searchTargetAudience.addEventListener('change', () => this.performSearch());
        }
        if (searchStatus) {
            searchStatus.addEventListener('change', () => this.performSearch());
        }
        if (btnClearSearch) {
            btnClearSearch.addEventListener('click', () => this.clearSearch());
        }
        if (btnRefresh) {
            btnRefresh.addEventListener('click', () => this.refreshData());
        }
        if (confirmDeleteBtn) {
            confirmDeleteBtn.addEventListener('click', () => this.deleteCourse());
        }
        if (confirmStatusBtn) {
            confirmStatusBtn.addEventListener('click', () => this.toggleCourseStatus());
        }
    }

    performSearch() {
        this.currentPage = 1;
        this.loadCourses();
    }

    clearSearch() {
        const searchName = document.getElementById('searchName');
        const searchTargetAudience = document.getElementById('searchTargetAudience');
        const searchStatus = document.getElementById('searchStatus');

        if (searchName) searchName.value = '';
        if (searchTargetAudience) searchTargetAudience.value = '';
        if (searchStatus) searchStatus.value = '';

        this.performSearch();
    }

    refreshData() {
        this.loadDashboardStats();
        this.loadCourses();
        this.showAlert('Đã làm mới dữ liệu', 'info');
    }

    buildApiUrl() {
        let url = `${this.apiBaseUrl}?$skip=${(this.currentPage - 1) * this.pageSize}&$top=${this.pageSize}&$count=true&$expand=UserCourses`;

        const filters = [];

        const searchName = document.getElementById('searchName');
        if (searchName && searchName.value.trim()) {
            filters.push(`contains(tolower(Title), '${searchName.value.toLowerCase()}')`);
        }

        const searchTargetAudience = document.getElementById('searchTargetAudience');
        if (searchTargetAudience && searchTargetAudience.value) {
            filters.push(`TargetAudience eq '${searchTargetAudience.value}'`);
        }

        const searchStatus = document.getElementById('searchStatus');
        if (searchStatus && searchStatus.value) {
            filters.push(`IsActive eq ${searchStatus.value}`);
        }

        if (filters.length > 0) {
            url += `&$filter=${filters.join(' and ')}`;
        }

        return url;
    }

    async loadDashboardStats() {
        try {
            const headers = {
                'Authorization': `Bearer ${this.authToken}`,
                'Content-Type': 'application/json'
            };

            const [coursesResponse, userCoursesResponse] = await Promise.all([
                fetch(`${this.apiBaseUrl}?$count=true&$top=0`, { headers }),
                fetch(`${this.userCoursesApiUrl}?$count=true&$top=0`, { headers })
            ]);

            const [coursesData, userCoursesData] = await Promise.all([
                coursesResponse.json(),
                userCoursesResponse.json()
            ]);

            const activeCoursesResponse = await fetch(`${this.apiBaseUrl}?$filter=IsActive eq true&$count=true&$top=0`, { headers });
            const activeCoursesData = await activeCoursesResponse.json();

            const totalCourses = coursesData['@odata.count'] || 0;
            const activeCourses = activeCoursesData['@odata.count'] || 0;
            const inactiveCourses = totalCourses - activeCourses;
            const totalRegistrations = userCoursesData['@odata.count'] || 0;

            this.updateDashboardStats({
                totalCourses,
                activeCourses,
                inactiveCourses,
                totalRegistrations
            });

        } catch (error) {
            console.error('Error loading dashboard stats:', error);
        }
    }

    updateDashboardStats(stats) {
        this.stats = stats;

        const totalCoursesEl = document.getElementById('totalCourses');
        const activeCoursesEl = document.getElementById('activeCourses');
        const inactiveCoursesEl = document.getElementById('inactiveCourses');
        const totalRegistrationsEl = document.getElementById('totalRegistrations');

        if (totalCoursesEl) totalCoursesEl.textContent = stats.totalCourses;
        if (activeCoursesEl) activeCoursesEl.textContent = stats.activeCourses;
        if (inactiveCoursesEl) inactiveCoursesEl.textContent = stats.inactiveCourses;
        if (totalRegistrationsEl) totalRegistrationsEl.textContent = stats.totalRegistrations;
    }

    async loadCourses() {
        try {
            this.showLoading();

            const response = await fetch(this.buildApiUrl(), {
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();

            this.renderCourses(data.value || []);
            this.renderPagination(data['@odata.count'] || 0);

        } catch (error) {
            console.error('Error loading courses:', error);
            this.showAlert('Có lỗi xảy ra khi tải danh sách khóa học', 'danger');
            this.showEmptyState();
        }
    }

    showLoading() {
        const tbody = document.getElementById('coursesTableBody');
        if (tbody) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center">
                        <div class="spinner-border" role="status">
                            <span class="visually-hidden">Đang tải...</span>
                        </div>
                    </td>
                </tr>
            `;
        }
    }

    renderCourses(courses) {
        const tbody = document.getElementById('coursesTableBody');
        if (!tbody) return;

        if (courses.length === 0) {
            this.showEmptyState();
            return;
        }

        const rows = courses.map(course => this.createCourseRow(course)).join('');
        tbody.innerHTML = rows;
    }

    createCourseRow(course) {
        const statusBadge = course.IsActive
            ? '<span class="badge bg-success">Hoạt động</span>'
            : '<span class="badge bg-secondary">Không hoạt động</span>';

        const registrationCount = course.UserCourses ? course.UserCourses.length : 0;
        const createdDate = course.CreatedAt ? new Date(course.CreatedAt).toLocaleDateString('vi-VN') : '';
        const durationInHours = course.DurationMinutes ? Math.round(course.DurationMinutes / 60 * 10) / 10 : 0;

        const statusToggleBtn = course.IsActive
            ? `<button class="btn btn-sm btn-warning" onclick="courseManage.showStatusModal(${course.CourseId}, '${this.escapeHtml(course.Title)}', false)" title="Tạm dừng">
                 <i class="fas fa-pause"></i>
               </button>`
            : `<button class="btn btn-sm btn-success" onclick="courseManage.showStatusModal(${course.CourseId}, '${this.escapeHtml(course.Title)}', true)" title="Kích hoạt">
                 <i class="fas fa-play"></i>
               </button>`;

        const shortDescription = course.Description ? (course.Description.length > 50 ? course.Description.substring(0, 50) + '...' : course.Description) : '';

        return `
            <tr>
                <td><strong>#${course.CourseId}</strong></td>
                <td>
                    <div class="fw-bold">${this.escapeHtml(course.Title || '')}</div>
                    <small class="text-muted">${this.escapeHtml(shortDescription)}</small>
                </td>
                <td><span class="badge bg-primary">${this.escapeHtml(course.TargetAudience || '')}</span></td>
                <td>${durationInHours} giờ</td>
                <td>${statusBadge}</td>
                <td>${createdDate}</td>
                <td><span class="badge bg-info">${registrationCount}</span></td>
                <td>
                    <div class="btn-group" role="group">
                        <a href="/Courses/Edit/${course.CourseId}" class="btn btn-sm btn-outline-primary" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </a>
                        ${statusToggleBtn}
                        <button class="btn btn-sm btn-outline-danger" onclick="courseManage.showDeleteModal(${course.CourseId}, '${this.escapeHtml(course.Title)}')" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }

    escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    showEmptyState() {
        const tbody = document.getElementById('coursesTableBody');
        if (tbody) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="8" class="text-center py-5">
                        <div class="empty-state">
                            <i class="fas fa-graduation-cap fa-4x text-muted mb-3"></i>
                            <h5 class="text-muted">Không có khóa học nào</h5>
                            <p class="text-muted">Không tìm thấy khóa học phù hợp với tiêu chí tìm kiếm</p>
                            <a href="/Courses/Create" class="btn btn-primary">
                                <i class="fas fa-plus"></i> Tạo khóa học đầu tiên
                            </a>
                        </div>
                    </td>
                </tr>
            `;
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
            paginationHtml += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="courseManage.goToPage(1)">Đầu</a>
                </li>
                <li class="page-item">
                    <a class="page-link" href="#" onclick="courseManage.goToPage(${this.currentPage - 1})">Trước</a>
                </li>
            `;
        }

        for (let i = Math.max(1, this.currentPage - 2); i <= Math.min(this.totalPages, this.currentPage + 2); i++) {
            const activeClass = i === this.currentPage ? 'active' : '';
            paginationHtml += `
                <li class="page-item ${activeClass}">
                    <a class="page-link" href="#" onclick="courseManage.goToPage(${i})">${i}</a>
                </li>
            `;
        }

        if (this.currentPage < this.totalPages) {
            paginationHtml += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="courseManage.goToPage(${this.currentPage + 1})">Sau</a>
                </li>
                <li class="page-item">
                    <a class="page-link" href="#" onclick="courseManage.goToPage(${this.totalPages})">Cuối</a>
                </li>
            `;
        }

        pagination.innerHTML = paginationHtml;
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadCourses();
    }

    showDeleteModal(courseId, courseName) {
        this.currentCourseId = courseId;
        const deleteTitle = document.getElementById('deleteCourseTitle');
        if (deleteTitle) {
            deleteTitle.textContent = courseName;
        }

        const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
        modal.show();
    }

    showStatusModal(courseId, courseName, newStatus) {
        this.currentCourseId = courseId;
        this.newStatus = newStatus;

        const statusCourseTitle = document.getElementById('statusCourseTitle');
        const statusAction = document.getElementById('statusAction');

        if (statusCourseTitle) statusCourseTitle.textContent = courseName;
        if (statusAction) statusAction.textContent = newStatus ? 'kích hoạt' : 'tạm dừng';

        const modal = new bootstrap.Modal(document.getElementById('statusModal'));
        modal.show();
    }

    async deleteCourse() {
        if (!this.currentCourseId) return;

        try {
            const response = await fetch(`${this.apiBaseUrl}(${this.currentCourseId})`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Không thể xóa khóa học');
            }

            this.showAlert('Xóa khóa học thành công', 'success');
            this.loadDashboardStats();
            this.loadCourses();

            const modal = bootstrap.Modal.getInstance(document.getElementById('deleteModal'));
            if (modal) modal.hide();

        } catch (error) {
            console.error('Error deleting course:', error);
            this.showAlert('Có lỗi xảy ra khi xóa khóa học', 'danger');
        }
    }

    async toggleCourseStatus() {
        if (!this.currentCourseId || this.newStatus === undefined) return;

        try {
            const response = await fetch(`${this.apiBaseUrl}(${this.currentCourseId})`, {
                method: 'PATCH',
                headers: {
                    'Authorization': `Bearer ${this.authToken}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    IsActive: this.newStatus
                })
            });

            if (!response.ok) {
                throw new Error('Không thể thay đổi trạng thái khóa học');
            }

            const statusText = this.newStatus ? 'kích hoạt' : 'tạm dừng';
            this.showAlert(`Đã ${statusText} khóa học thành công`, 'success');

            this.loadDashboardStats();
            this.loadCourses();

            const modal = bootstrap.Modal.getInstance(document.getElementById('statusModal'));
            if (modal) modal.hide();

        } catch (error) {
            console.error('Error toggling course status:', error);
            this.showAlert('Có lỗi xảy ra khi thay đổi trạng thái khóa học', 'danger');
        }
    }

    showAlert(message, type = 'info') {
        const alertContainer = document.getElementById('alertContainer');
        if (!alertContainer) return;

        const alertId = 'alert-' + Date.now();
        const iconClass = type === 'success' ? 'check-circle' : (type === 'danger' ? 'exclamation-triangle' : 'info-circle');

        const alertHtml = `
            <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
                <i class="fas fa-${iconClass}"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        alertContainer.insertAdjacentHTML('beforeend', alertHtml);

        setTimeout(() => {
            const alertElement = document.getElementById(alertId);
            if (alertElement) {
                alertElement.remove();
            }
        }, 5000);
    }
}

const courseManage = new CourseManage();