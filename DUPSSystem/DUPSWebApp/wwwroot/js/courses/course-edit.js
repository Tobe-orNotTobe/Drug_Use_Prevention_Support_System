class CourseEdit {
    constructor() {
        this.apiBaseUrl = this.getApiBaseUrl() + '/odata/Courses';
        this.authToken = this.getAuthToken();
        this.courseId = this.getCourseIdFromUrl();
        this.originalData = {};

        this.init();
    }

    getApiBaseUrl() {
        const apiUrl = document.querySelector('meta[name="api-base-url"]');
        return apiUrl ? apiUrl.getAttribute('content') : 'https://localhost:7008';
    }

    init() {
        if (!this.courseId) {
            this.showAlert('Khong tim thay ID khoa hoc', 'danger');
            return;
        }

        this.bindEvents();
        this.loadCourseData();
    }

    getAuthToken() {
        const tokenElement = document.querySelector('meta[name="auth-token"]');
        return tokenElement ? tokenElement.getAttribute('content') || '' : '';
    }

    getCourseIdFromUrl() {
        const pathParts = window.location.pathname.split('/');
        return pathParts[pathParts.length - 1];
    }

    bindEvents() {
        const editForm = document.getElementById('editCourseForm');
        const resetBtn = document.getElementById('resetBtn');
        const descriptionField = document.getElementById('description');

        if (editForm) {
            editForm.addEventListener('submit', (e) => this.handleSubmit(e));
        }

        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.resetToOriginal());
        }

        if (descriptionField) {
            descriptionField.addEventListener('input', () => this.updateCharacterCount());
        }

        this.setupValidation();
    }

    setupValidation() {
        const form = document.getElementById('editCourseForm');
        if (!form) return;

        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });
    }

    async loadCourseData() {
        try {
            const response = await fetch(this.apiBaseUrl + '(' + this.courseId + ')', {
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                }
            });

            if (!response.ok) {
                throw new Error('Khong the tai thong tin khoa hoc');
            }

            const course = await response.json();
            this.populateForm(course);
            this.originalData = {};
            for (let key in course) {
                this.originalData[key] = course[key];
            }

            const loadingSpinner = document.getElementById('loadingSpinner');
            const editForm = document.getElementById('editCourseForm');

            if (loadingSpinner) loadingSpinner.style.display = 'none';
            if (editForm) editForm.style.display = 'block';

        } catch (error) {
            console.error('Error loading course:', error);
            this.showAlert('Co loi xay ra khi tai thong tin khoa hoc', 'danger');

            setTimeout(() => {
                window.location.href = '/Courses/Index';
            }, 2000);
        }
    }

    populateForm(course) {
        const courseIdField = document.getElementById('courseId');
        const courseTitleField = document.getElementById('courseTitle');
        const descriptionField = document.getElementById('description');
        const targetAudienceField = document.getElementById('targetAudience');
        const durationField = document.getElementById('durationMinutes');
        const isActiveField = document.getElementById('isActive');
        const displayCourseId = document.getElementById('displayCourseId');
        const displayCreatedAt = document.getElementById('displayCreatedAt');

        if (courseIdField) courseIdField.value = course.CourseId || '';
        if (courseTitleField) courseTitleField.value = course.Title || '';
        if (descriptionField) descriptionField.value = course.Description || '';
        if (targetAudienceField) targetAudienceField.value = course.TargetAudience || '';
        if (durationField) durationField.value = course.DurationMinutes || '';
        if (isActiveField) isActiveField.checked = course.IsActive || false;

        if (displayCourseId) displayCourseId.textContent = course.CourseId || '';
        if (displayCreatedAt) {
            displayCreatedAt.textContent = course.CreatedAt
                ? new Date(course.CreatedAt).toLocaleString('vi-VN')
                : 'Khong xac dinh';
        }

        this.updateCharacterCount();
    }

    validateField(field) {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';

        switch (field.name) {
            case 'Title':
                if (!value) {
                    isValid = false;
                    errorMessage = 'Tieu de khoa hoc la bat buoc';
                } else if (value.length > 255) {
                    isValid = false;
                    errorMessage = 'Tieu de khoa hoc khong duoc vuot qua 255 ky tu';
                }
                break;

            case 'TargetAudience':
                if (!value) {
                    isValid = false;
                    errorMessage = 'Vui long chon doi tuong';
                }
                break;

            case 'DurationMinutes':
                const duration = parseInt(value);
                if (!value) {
                    isValid = false;
                    errorMessage = 'Thoi luong la bat buoc';
                } else if (isNaN(duration) || duration < 1) {
                    isValid = false;
                    errorMessage = 'Thoi luong phai la so duong';
                } else if (duration > 10080) {
                    isValid = false;
                    errorMessage = 'Thoi luong khong duoc vuot qua 10080 phut (1 tuan)';
                }
                break;
        }

        this.setFieldValidation(field, isValid, errorMessage);
        return isValid;
    }

    setFieldValidation(field, isValid, errorMessage) {
        const feedbackElement = field.parentElement.querySelector('.invalid-feedback');

        if (isValid) {
            field.classList.remove('is-invalid');
            field.classList.add('is-valid');
            if (feedbackElement) {
                feedbackElement.textContent = '';
            }
        } else {
            field.classList.remove('is-valid');
            field.classList.add('is-invalid');
            if (feedbackElement) {
                feedbackElement.textContent = errorMessage;
            }
        }
    }

    clearFieldError(field) {
        field.classList.remove('is-invalid', 'is-valid');
        const feedbackElement = field.parentElement.querySelector('.invalid-feedback');
        if (feedbackElement) {
            feedbackElement.textContent = '';
        }
    }

    updateCharacterCount() {
        const textarea = document.getElementById('description');
        if (!textarea) return;

        const currentLength = textarea.value.length;

        let helpText = textarea.parentElement.querySelector('.form-text');
        if (helpText) {
            const originalText = 'Mo ta chi tiet ve noi dung khoa hoc';
            helpText.textContent = originalText + ' (' + currentLength + ' ky tu)';
        }
    }

    validateForm() {
        const form = document.getElementById('editCourseForm');
        if (!form) return false;

        const requiredFields = form.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!this.validateField(field)) {
                isValid = false;
            }
        });

        return isValid;
    }

    async handleSubmit(e) {
        e.preventDefault();

        if (!this.validateForm()) {
            this.showAlert('Vui long kiem tra lai thong tin nhap vao', 'danger');
            return;
        }

        const submitBtn = document.getElementById('submitBtn');
        const originalText = submitBtn ? submitBtn.innerHTML : '';

        try {
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Dang cap nhat...';
            }

            const courseData = this.getFormData();

            const response = await fetch(this.apiBaseUrl + '(' + this.courseId + ')', {
                method: 'PUT',
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(courseData)
            });

            // Cải thiện xử lý response
            if (!response.ok) {
                let errorMessage = 'Khong the cap nhat khoa hoc';

                // Kiểm tra content-type trước khi parse JSON
                const contentType = response.headers.get('content-type');
                if (contentType && contentType.includes('application/json')) {
                    try {
                        const errorData = await response.json();
                        errorMessage = errorData.message || errorMessage;
                    } catch (jsonError) {
                        console.error('Error parsing JSON response:', jsonError);
                    }
                } else {
                    // Nếu không phải JSON, lấy text response
                    try {
                        const errorText = await response.text();
                        if (errorText) {
                            errorMessage = `Loi server (${response.status}): ${errorText}`;
                        }
                    } catch (textError) {
                        errorMessage = `Loi server (${response.status})`;
                    }
                }

                throw new Error(errorMessage);
            }

            // Parse response data nếu thành công
            let result = null;
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                try {
                    result = await response.json();
                } catch (jsonError) {
                    console.log('Response is not JSON, but request was successful');
                }
            }

            this.showAlert('Cap nhat khoa hoc thanh cong!', 'success');

            setTimeout(() => {
                window.location.href = '/Courses/Index';
            }, 1500);

        } catch (error) {
            console.error('Error updating course:', error);
            this.showAlert(error.message || 'Co loi xay ra khi cap nhat khoa hoc', 'danger');
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }
    }
    getFormData() {
        const form = document.getElementById('editCourseForm');
        if (!form) return {};

        const courseIdField = form.querySelector('#courseId');
        const titleField = form.querySelector('#courseTitle');
        const descriptionField = form.querySelector('#description');
        const targetAudienceField = form.querySelector('#targetAudience');
        const durationField = form.querySelector('#durationMinutes');
        const isActiveField = form.querySelector('#isActive');

        return {
            CourseId: courseIdField ? parseInt(courseIdField.value) : parseInt(this.courseId),
            Title: titleField ? titleField.value.trim() : '',
            Description: descriptionField ? descriptionField.value.trim() || null : null,
            TargetAudience: targetAudienceField ? targetAudienceField.value : '',
            DurationMinutes: durationField ? parseInt(durationField.value) : 0,
            IsActive: isActiveField ? isActiveField.checked : false,
            CreatedAt: this.originalData.CreatedAt,
            UpdatedAt: new Date().toISOString()
        };
    }

    resetToOriginal() {
        if (Object.keys(this.originalData).length === 0) {
            this.showAlert('Chua co du lieu goc de khoi phuc', 'warning');
            return;
        }

        this.populateForm(this.originalData);

        const form = document.getElementById('editCourseForm');
        if (form) {
            const inputs = form.querySelectorAll('input, select, textarea');
            inputs.forEach(input => {
                input.classList.remove('is-valid', 'is-invalid');
            });

            const feedbacks = form.querySelectorAll('.invalid-feedback');
            feedbacks.forEach(feedback => {
                feedback.textContent = '';
            });
        }

        this.showAlert('Da khoi phuc du lieu goc', 'info');
    }

    hasChanges() {
        if (Object.keys(this.originalData).length === 0) return false;

        const currentData = this.getFormData();

        return (
            currentData.Title !== this.originalData.Title ||
            currentData.Description !== this.originalData.Description ||
            currentData.TargetAudience !== this.originalData.TargetAudience ||
            currentData.DurationMinutes !== this.originalData.DurationMinutes ||
            currentData.IsActive !== this.originalData.IsActive
        );
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

window.addEventListener('beforeunload', function (e) {
    if (window.courseEdit && window.courseEdit.hasChanges()) {
        e.preventDefault();
        e.returnValue = 'Ban co nhung thay doi chua duoc luu. Ban co chac chan muon roi khoi trang?';
    }
});

document.addEventListener('DOMContentLoaded', function () {
    window.courseEdit = new CourseEdit();
});