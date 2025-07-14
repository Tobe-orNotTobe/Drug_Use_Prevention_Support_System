
// Global utility functions
window.DUPSUtils = {
    // Format date to Vietnamese locale
    formatDate: function (dateString) {
        if (!dateString) return 'Không rõ';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    },

    // Format datetime to Vietnamese locale
    formatDateTime: function (dateString) {
        if (!dateString) return 'Không rõ';
        const date = new Date(dateString);
        return date.toLocaleString('vi-VN');
    },

    // Truncate text
    truncateText: function (text, maxLength) {
        if (!text || text.length <= maxLength) return text;
        return text.substring(0, maxLength) + '...';
    },

    // Get age group display text
    getAgeGroupDisplay: function (ageGroup) {
        const ageGroups = {
            'Students': 'Học sinh',
            'Parents': 'Phụ huynh',
            'Teachers': 'Giáo viên',
            'Adults': 'Người lớn'
        };
        return ageGroups[ageGroup] || ageGroup || 'Không xác định';
    },

    // Get level display text
    getLevelDisplay: function (level) {
        const levels = {
            'Beginner': 'Cơ bản',
            'Intermediate': 'Trung cấp',
            'Advanced': 'Nâng cao'
        };
        return levels[level] || level || 'Không xác định';
    },

    // Get status color for badges
    getStatusColor: function (status) {
        const colors = {
            'Active': 'success',
            'Completed': 'primary',
            'Cancelled': 'danger',
            'Suspended': 'warning',
            'Pending': 'info'
        };
        return colors[status] || 'secondary';
    },

    // Get auth token
    getAuthToken: function () {
        return window.AUTH_TOKEN || '';
    },

    // Show success message
    showSuccessMessage: function (message) {
        if (typeof toastr !== 'undefined') {
            toastr.success(message);
        } else {
            alert('✅ ' + message);
        }
    },

    // Show error message
    showErrorMessage: function (message) {
        if (typeof toastr !== 'undefined') {
            toastr.error(message);
        } else {
            alert('❌ ' + message);
        }
    },

    // Show info message
    showInfoMessage: function (message) {
        if (typeof toastr !== 'undefined') {
            toastr.info(message);
        } else {
            alert('ℹ️ ' + message);
        }
    },

    // Show warning message
    showWarningMessage: function (message) {
        if (typeof toastr !== 'undefined') {
            toastr.warning(message);
        } else {
            alert('⚠️ ' + message);
        }
    },

    // AJAX error handler
    handleAjaxError: function (xhr, defaultMessage) {
        let errorMessage = defaultMessage || 'Có lỗi xảy ra. Vui lòng thử lại.';

        if (xhr.responseJSON && xhr.responseJSON.message) {
            errorMessage = xhr.responseJSON.message;
        } else if (xhr.status === 401) {
            errorMessage = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.';
            // Redirect to login
            setTimeout(() => {
                window.location.href = '/Auth/Login';
            }, 2000);
        } else if (xhr.status === 403) {
            errorMessage = 'Bạn không có quyền thực hiện hành động này.';
        } else if (xhr.status === 404) {
            errorMessage = 'Không tìm thấy dữ liệu yêu cầu.';
        } else if (xhr.status === 500) {
            errorMessage = 'Lỗi máy chủ. Vui lòng liên hệ quản trị viên.';
        }

        this.showErrorMessage(errorMessage);
        return errorMessage;
    },

    // Confirm dialog
    confirm: function (message, callback) {
        if (typeof bootbox !== 'undefined') {
            bootbox.confirm({
                message: message,
                buttons: {
                    confirm: {
                        label: 'Xác nhận',
                        className: 'btn-primary'
                    },
                    cancel: {
                        label: 'Hủy bỏ',
                        className: 'btn-secondary'
                    }
                },
                callback: callback
            });
        } else {
            const result = confirm(message);
            if (callback) callback(result);
        }
    },

    // Loading overlay
    showLoading: function (element) {
        const $element = $(element || 'body');
        const loadingHtml = `
            <div class="loading-overlay">
                <div class="loading-content">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Đang tải...</span>
                    </div>
                    <p class="mt-2">Đang xử lý...</p>
                </div>
            </div>
        `;

        $element.append(loadingHtml);

        // Add CSS if not exists
        if (!$('#loading-overlay-styles').length) {
            $('head').append(`
                <style id="loading-overlay-styles">
                .loading-overlay {
                    position: fixed;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(255, 255, 255, 0.8);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    z-index: 9999;
                }
                .loading-content {
                    text-align: center;
                    background: white;
                    padding: 30px;
                    border-radius: 10px;
                    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                }
                </style>
            `);
        }
    },

    hideLoading: function () {
        $('.loading-overlay').remove();
    },

    // Validate form
    validateForm: function (formSelector) {
        const $form = $(formSelector);
        let isValid = true;

        // Clear previous validation
        $form.find('.form-control, .form-select').removeClass('is-invalid');
        $form.find('.invalid-feedback').text('');

        // Check required fields
        $form.find('[required]').each(function () {
            const $field = $(this);
            const value = $field.val().trim();

            if (!value) {
                $field.addClass('is-invalid');
                const label = $field.closest('.form-group, .mb-3').find('label').text().replace(' *', '');
                $field.siblings('.invalid-feedback').text(`${label} là bắt buộc`);
                isValid = false;
            }
        });

        // Check email fields
        $form.find('input[type="email"]').each(function () {
            const $field = $(this);
            const email = $field.val().trim();

            if (email && !isValidEmail(email)) {
                $field.addClass('is-invalid');
                $field.siblings('.invalid-feedback').text('Email không hợp lệ');
                isValid = false;
            }
        });

        // Focus first invalid field
        if (!isValid) {
            $form.find('.is-invalid').first().focus();
        }

        return isValid;
    },

    // Generate pagination HTML
    generatePagination: function (totalCount, currentPage, pageSize) {
        const totalPages = Math.ceil(totalCount / pageSize);

        if (totalPages <= 1) {
            return '';
        }

        let html = '<nav><ul class="pagination justify-content-center">';

        // Previous button
        html += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${currentPage - 1}">Trước</a>
                 </li>`;

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
                html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                            <a class="page-link" href="#" data-page="${i}">${i}</a>
                         </li>`;
            } else if (i === currentPage - 3 || i === currentPage + 3) {
                html += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }
        }

        // Next button
        html += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${currentPage + 1}">Sau</a>
                 </li>`;

        html += '</ul></nav>';
        return html;
    }
};

// Helper functions
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Initialize global settings when document is ready
$(document).ready(function () {
    // Configure toastr if available
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            closeButton: true,
            debug: false,
            newestOnTop: true,
            progressBar: true,
            positionClass: 'toast-top-right',
            preventDuplicates: false,
            showDuration: 300,
            hideDuration: 1000,
            timeOut: 5000,
            extendedTimeOut: 1000,
            showEasing: 'swing',
            hideEasing: 'linear',
            showMethod: 'fadeIn',
            hideMethod: 'fadeOut'
        };
    }

    // Global AJAX setup
    $.ajaxSetup({
        beforeSend: function (xhr) {
            const token = window.DUPSUtils.getAuthToken();
            if (token) {
                xhr.setRequestHeader('Authorization', 'Bearer ' + token);
            }
        },
        error: function (xhr, status, error) {
            if (xhr.status === 401) {
                window.DUPSUtils.handleAjaxError(xhr);
            }
        }
    });

    // Handle browser back button
    window.addEventListener('popstate', function (event) {
        // Reload page if needed for state management
        // location.reload();
    });
});