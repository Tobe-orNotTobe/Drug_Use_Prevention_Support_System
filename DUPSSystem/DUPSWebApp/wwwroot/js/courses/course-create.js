class CourseCreate {
    constructor() {
        this.apiBaseUrl = this.getApiBaseUrl() + '/odata/Courses';
        this.authToken = this.getAuthToken();
        this.originalFormData = {};

        this.init();
    }

    getApiBaseUrl() {
        const apiUrl = document.querySelector('meta[name="api-base-url"]');
        return apiUrl ? apiUrl.getAttribute('content') : 'https://localhost:7008';
    }

    init() {
        this.bindEvents();
        this.saveOriginalFormData();
    }

    getAuthToken() {
        const tokenElement = document.querySelector('meta[name="auth-token"]');
        return tokenElement ? tokenElement.getAttribute('content') || '' : '';
    }

    bindEvents() {
        const createForm = document.getElementById('createCourseForm');
        const resetBtn = document.getElementById('resetBtn');
        const descriptionField = document.getElementById('description');

        if (createForm) {
            createForm.addEventListener('submit', (e) => this.handleSubmit(e));
        }

        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.resetForm());
        }

        if (descriptionField) {
            descriptionField.addEventListener('input', () => this.updateCharacterCount());
        }

        this.setupValidation();
    }

    saveOriginalFormData() {
        const form = document.getElementById('createCourseForm');
        if (!form) return;

        const formData = new FormData(form);

        for (let [key, value] of formData.entries()) {
            this.originalFormData[key] = value;
        }

        const isActiveField = document.getElementById('isActive');
        if (isActiveField) {
            this.originalFormData['IsActive'] = isActiveField.checked;
        }
    }

    setupValidation() {
        const form = document.getElementById('createCourseForm');
        if (!form) return;

        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });
    }

    validateField(field) {
        const value = field.value.trim();
        let isValid = true;
        let errorMessage = '';

        switch (field.name) {
            case 'Title':
                if (!value) {
                    isValid = false;
                    errorMessage = 'Tiêu đề khóa học là bắt buộc';
                } else if (value.length > 255) {
                    isValid = false;
                    errorMessage = 'Tiêu đề khóa học không được vượt quá 255 ký tự';
                }
                break;

            case 'TargetAudience':
                if (!value) {
                    isValid = false;
                    errorMessage = 'Vui lòng chọn đối tượng';
                }
                break;

            case 'DurationMinutes':
                const duration = parseInt(value);
                if (!value) {
                    isValid = false;
                    errorMessage = 'Thời lượng là bắt buộc';
                } else if (isNaN(duration) || duration < 1) {
                    isValid = false;
                    errorMessage = 'Thời lượng phải là số dương';
                } else if (duration > 10080) {
                    isValid = false;
                    errorMessage = 'Thời lượng không được vượt quá 10080 phút (1 tuần)';
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
            const originalText = 'Mô tả chi tiết về nội dung khóa học';
            helpText.textContent = originalText + ' (' + currentLength + ' ký tự)';
        }
    }

    validateForm() {
        const form = document.getElementById('createCourseForm');
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
            this.showAlert('Vui lòng kiểm tra lại thông tin nhập vào', 'danger');
            return;
        }

        const submitBtn = document.getElementById('submitBtn');
        const originalText = submitBtn ? submitBtn.innerHTML : '';

        try {
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang tạo...';
            }

            const courseData = this.getFormData();

            const response = await fetch(this.apiBaseUrl, {
                method: 'POST',
                headers: {
                    'Authorization': 'Bearer ' + this.authToken,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(courseData)
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Không thể tạo khóa học');
            }

            const result = await response.json();

            this.showAlert('Tạo khóa học thành công!', 'success');

            setTimeout(() => {
                window.location.href = '/Courses/Index';
            }, 1500);

        } catch (error) {
            console.error('Error creating course:', error);
            this.showAlert(error.message || 'Có lỗi xảy ra khi tạo khóa học', 'danger');
        } finally {
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }
    }

    getFormData() {
        const form = document.getElementById('createCourseForm');
        if (!form) return {};

        const titleField = form.querySelector('#courseTitle');
        const descriptionField = form.querySelector('#description');
        const targetAudienceField = form.querySelector('#targetAudience');
        const durationField = form.querySelector('#durationMinutes');
        const isActiveField = form.querySelector('#isActive');

        return {
            Title: titleField ? titleField.value.trim() : '',
            Description: descriptionField ? descriptionField.value.trim() || null : null,
            TargetAudience: targetAudienceField ? targetAudienceField.value : '',
            DurationMinutes: durationField ? parseInt(durationField.value) : 0,
            IsActive: isActiveField ? isActiveField.checked : true
        };
    }

    resetForm() {
        const form = document.getElementById('createCourseForm');
        if (!form) return;

        form.reset();

        const isActiveField = document.getElementById('isActive');
        if (isActiveField) {
            isActiveField.checked = true;
        }

        const inputs = form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            input.classList.remove('is-valid', 'is-invalid');
        });

        const feedbacks = form.querySelectorAll('.invalid-feedback');
        feedbacks.forEach(feedback => {
            feedback.textContent = '';
        });

        this.updateCharacterCount();

        this.showAlert('Đã đặt lại form', 'info');
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

document.addEventListener('DOMContentLoaded', function () {
    new CourseCreate();
});