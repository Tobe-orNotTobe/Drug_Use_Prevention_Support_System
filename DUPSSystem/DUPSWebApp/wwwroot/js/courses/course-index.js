class CourseIndex {
    constructor() {
        this.apiBaseUrl = this.getApiBaseUrl() + '/odata/Courses';
        this.userCoursesApiUrl = this.getApiBaseUrl() + '/odata/UserCourses';
        this.currentPage = 1;
        this.pageSize = 10;
        this.totalPages = 0;
        this.currentCourseId = null;
        this.currentRole = this.getCurrentUserRole();
        this.authToken = this.getAuthToken();

        this.init();
    }

    getApiBaseUrl() {
        const apiUrl = document.querySelector('meta[name="api-base-url"]');
        return apiUrl ? apiUrl.getAttribute('content') : 'https://localhost:7008';
    }

    init() {
        this.bindEvents();
        this.loadCourses();
    }

    getCurrentUserRole() {
        const bodyElement = document.querySelector('body');
        return bodyElement ? bodyElement.getAttribute('data-user-role') || 'Guest' : 'Guest';
    }

    getAuthToken() {
        const tokenElement = document.querySelector('meta[name="auth-token"]');
        return tokenElement ? tokenElement.getAttribute('content') || '' : '';
    }

    bindEvents() {
        const btnSearch = document.getElementById('btnSearch');
        const searchName = document.getElementById('searchName');
        const searchTargetAudience = document.getElementById('searchTargetAudience');
        const searchStatus = document.getElementById('searchStatus');
        const btnClearSearch = document.getElementById('btnClearSearch');
        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

        if (btnSearch) btnSearch.addEventListener('click', () => this.performSearch());
        if (searchName) {
            searchName.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') this.performSearch();
            });
        }
        if (searchTargetAudience) searchTargetAudience.addEventListener('change', () => this.performSearch());
        if (searchStatus) searchStatus.addEventListener('change', () => this.performSearch());
        if (btnClearSearch) btnClearSearch.addEventListener('click', () => this.clearSearch());
        if (confirmDeleteBtn) confirmDeleteBtn.addEventListener('click', () => this.deleteCourse());

        document.addEventListener('click', (e) => {
            if (e.target && e.target.id === 'registerCourseBtn') {
                this.registerCourse();
            }
            if (e.target && e.target.classList.contains('course-register-btn')) {
                const courseId = e.target.getAttribute('data-course-id');
                this.registerForCourse(parseInt(courseId), e.target);
            }
        });
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

    buildApiUrl() {
        let url = this.apiBaseUrl + '?$skip=' + ((this.currentPage - 1) * this.pageSize) + '&$top=' + this.pageSize + '&$count=true';

        const filters = [];

        const searchName = document.getElementById('searchName');
        if (searchName && searchName.value.trim()) {
            filters.push("contains(tolower(Title), '" + searchName.value.toLowerCase() + "')");
        }

        const searchTargetAudience = document.getElementById('searchTargetAudience');
        if (searchTargetAudience && searchTargetAudience.value) {
            filters.push("TargetAudience eq '" + searchTargetAudience.value + "'");
        }

        const searchStatus = document.getElementById('searchStatus');
        if (searchStatus && searchStatus.value) {
            filters.push("IsActive eq " + searchStatus.value);
        }

        if (filters.length > 0) {
            url += '&$filter=' + filters.join(' and ');
        }

        return url;
    }

    async loadCourses() {
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
            console.error('Error loading courses:', error);
            this.showAlert('Co loi xay ra khi tai danh sach khoa hoc', 'danger');
            this.showEmptyState();
        }
    }

    showLoading() {
        const tbody = document.getElementById('coursesTableBody');
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center"><div class="spinner-border" role="status"><span class="visually-hidden">Dang tai...</span></div></td></tr>';
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
            ? '<span class="badge bg-success">Hoat dong</span>'
            : '<span class="badge bg-secondary">Khong hoat dong</span>';

        const canManage = ['Staff', 'Manager', 'Admin'].includes(this.currentRole);
        const canRegister = this.currentRole === 'Member' && course.IsActive;

        let actionButtons = '<button class="btn btn-sm btn-info" onclick="courseIndex.viewCourse(' + course.CourseId + ')"><i class="fas fa-eye"></i></button>';

        if (canRegister) {
            actionButtons += '<button class="btn btn-sm btn-success course-register-btn" data-course-id="' + course.CourseId + '" title="Dang ky"><i class="fas fa-user-plus"></i> Đăng ký</button>';
        }

        if (canManage) {
            actionButtons += '<a href="/Courses/Edit/' + course.CourseId + '" class="btn btn-sm btn-warning" title="Chinh sua"><i class="fas fa-edit"></i></a>';
            actionButtons += '<button class="btn btn-sm btn-danger" onclick="courseIndex.showDeleteModal(' + course.CourseId + ', \'' + course.Title + '\')" title="Xoa"><i class="fas fa-trash"></i></button>';
        }

        const durationInHours = course.DurationMinutes ? Math.round(course.DurationMinutes / 60 * 10) / 10 : 0;

        return '<tr>' +
            '<td><strong>' + (course.Title || '') + '</strong></td>' +
            '<td>' + (course.Description ? (course.Description.length > 100 ? course.Description.substring(0, 100) + '...' : course.Description) : '') + '</td>' +
            '<td><span class="badge bg-primary">' + (course.TargetAudience || '') + '</span></td>' +
            '<td>' + durationInHours + ' gio</td>' +
            '<td>' + statusBadge + '</td>' +
            '<td><div class="btn-group" role="group">' + actionButtons + '</div></td>' +
            '</tr>';
    }

    showEmptyState() {
        const tbody = document.getElementById('coursesTableBody');
        if (tbody) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center py-5"><div class="empty-state"><i class="fas fa-graduation-cap fa-4x text-muted mb-3"></i><h5 class="text-muted">Khong co khoa hoc nao</h5><p class="text-muted">Khong tim thay khoa hoc phu hop voi tieu chi tim kiem</p></div></td></tr>';
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
            paginationHtml += '<li class="page-item"><a class="page-link" href="#" onclick="courseIndex.goToPage(' + (this.currentPage - 1) + ')">Truoc</a></li>';
        }

        for (let i = Math.max(1, this.currentPage - 2); i <= Math.min(this.totalPages, this.currentPage + 2); i++) {
            const activeClass = i === this.currentPage ? 'active' : '';
            paginationHtml += '<li class="page-item ' + activeClass + '"><a class="page-link" href="#" onclick="courseIndex.goToPage(' + i + ')">' + i + '</a></li>';
        }

        if (this.currentPage < this.totalPages) {
            paginationHtml += '<li class="page-item"><a class="page-link" href="#" onclick="courseIndex.goToPage(' + (this.currentPage + 1) + ')">Sau</a></li>';
        }

        pagination.innerHTML = paginationHtml;
    }

    goToPage(page) {
        this.currentPage = page;
        this.loadCourses();
    }

    async viewCourse(courseId) {
        try {
            const response = await fetch(this.apiBaseUrl + '(' + courseId + ')', {
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Khong the tai thong tin khoa hoc');
            }

            const course = await response.json();
            this.showCourseDetail(course);

        } catch (error) {
            console.error('Error viewing course:', error);
            this.showAlert('Co loi xay ra khi tai thong tin khoa hoc', 'danger');
        }
    }

    showCourseDetail(course) {
        const statusText = course.IsActive ? 'Hoat dong' : 'Khong hoat dong';
        const statusClass = course.IsActive ? 'success' : 'secondary';

        const modalBody = document.getElementById('detailModalBody');
        if (!modalBody) return;

        const createdDate = course.CreatedAt ? new Date(course.CreatedAt).toLocaleDateString('vi-VN') : '';
        const durationInHours = course.DurationMinutes ? Math.round(course.DurationMinutes / 60 * 10) / 10 : 0;

        modalBody.innerHTML =
            '<div class="row">' +
            '<div class="col-md-12">' +
            '<h4>' + (course.Title || '') + '</h4>' +
            '<p class="text-muted mb-3">' + (course.Description || '') + '</p>' +
            '</div>' +
            '</div>' +
            '<div class="row mb-3">' +
            '<div class="col-md-6">' +
            '<strong>Doi tuong:</strong>' +
            '<span class="badge bg-primary ms-1">' + (course.TargetAudience || '') + '</span>' +
            '</div>' +
            '<div class="col-md-6">' +
            '<strong>Thoi luong:</strong> ' + durationInHours + ' gio' +
            '</div>' +
            '</div>' +
            '<div class="row mb-3">' +
            '<div class="col-md-6">' +
            '<strong>Trang thai:</strong>' +
            '<span class="badge bg-' + statusClass + ' ms-1">' + statusText + '</span>' +
            '</div>' +
            '<div class="col-md-6">' +
            '<strong>Ngay tao:</strong> ' + createdDate +
            '</div>' +
            '</div>';

        this.currentCourseId = course.CourseId;

        const registerBtn = document.getElementById('registerCourseBtn');
        if (registerBtn && ['Member', 'Staff', 'Consultant', 'Manager', 'Admin'].includes(this.currentRole)) {
            registerBtn.style.display = 'inline-block';
        }

        const modal = new bootstrap.Modal(document.getElementById('detailModal'));
        modal.show();
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

    async deleteCourse() {
        if (!this.currentCourseId) return;

        try {
            const response = await fetch(this.apiBaseUrl + '(' + this.currentCourseId + ')', {
                method: 'DELETE',
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Khong the xoa khoa hoc');
            }

            this.showAlert('Xoa khoa hoc thanh cong', 'success');
            this.loadCourses();

            const modal = bootstrap.Modal.getInstance(document.getElementById('deleteModal'));
            if (modal) modal.hide();

        } catch (error) {
            console.error('Error deleting course:', error);
            this.showAlert('Co loi xay ra khi xoa khoa hoc', 'danger');
        }
    }

    async registerForCourse(courseId, buttonElement) {
        if (!courseId) return;

        const originalText = buttonElement.innerHTML;

        try {
            buttonElement.disabled = true;
            buttonElement.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang đăng ký...';

            const response = await fetch(this.userCoursesApiUrl, {
                method: 'POST',
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    CourseId: courseId,
                    CompletionStatus: 'NotStarted'
                })
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Khong the dang ky khoa hoc');
            }

            this.showAlert('Ban da dang ky khoa hoc thanh cong!', 'success');

            buttonElement.innerHTML = '<i class="fas fa-check"></i> Đã đăng ký';
            buttonElement.classList.remove('btn-success');
            buttonElement.classList.add('btn-secondary');

            setTimeout(() => {
                buttonElement.disabled = true;
            }, 100);

        } catch (error) {
            console.error('Error registering course:', error);
            this.showAlert('Co loi xay ra khi dang ky khoa hoc. Co the ban da dang ky khoa hoc nay roi.', 'danger');

            buttonElement.disabled = false;
            buttonElement.innerHTML = originalText;
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

var courseIndex = new CourseIndex();