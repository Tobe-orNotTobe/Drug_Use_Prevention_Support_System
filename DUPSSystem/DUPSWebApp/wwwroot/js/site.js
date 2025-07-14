// Global site JavaScript for DUPS System

// API Base URL
const API_BASE_URL = 'https://localhost:7008/api';

// Global notification function
function showNotification(message, type = 'info', duration = 5000) {
    const alertClass = getAlertClass(type);
    const icon = getAlertIcon(type);
    
    const notification = `
        <div class="alert ${alertClass} alert-dismissible fade show notification-alert" role="alert">
            <i class="${icon}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    // Remove existing notifications
    $('.notification-alert').remove();
    
    // Add new notification
    $('#notification-area').html(notification);
    
    // Auto dismiss
    if (duration > 0) {
        setTimeout(() => {
            $('.notification-alert').alert('close');
        }, duration);
    }
}

function getAlertClass(type) {
    switch (type) {
        case 'success': return 'alert-success';
        case 'error': case 'danger': return 'alert-danger';
        case 'warning': return 'alert-warning';
        case 'info': default: return 'alert-info';
    }
}

function getAlertIcon(type) {
    switch (type) {
        case 'success': return 'fas fa-check-circle';
        case 'error': case 'danger': return 'fas fa-exclamation-triangle';
        case 'warning': return 'fas fa-exclamation-circle';
        case 'info': default: return 'fas fa-info-circle';
    }
}

// Loading overlay functions
function showLoading(message = 'Đang xử lý...') {
    const loadingHtml = `
        <div id="loading-overlay" class="loading-overlay">
            <div class="loading-content">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <div class="mt-2">${message}</div>
            </div>
        </div>
    `;
    
    $('body').append(loadingHtml);
}

function hideLoading() {
    $('#loading-overlay').remove();
}

// Confirmation dialog
function confirmAction(message, callback, title = 'Xác nhận') {
    if (confirm(`${title}\n\n${message}`)) {
        callback();
    }
}

// Advanced confirmation modal (requires Bootstrap 5)
function showConfirmModal(title, message, onConfirm, onCancel = null) {
    const modalId = 'confirmModal_' + Date.now();
    const modalHtml = `
        <div class="modal fade" id="${modalId}" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        ${message}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>
                        <button type="button" class="btn btn-primary confirm-btn">Xác nhận</button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    $('body').append(modalHtml);
    const modal = new bootstrap.Modal(document.getElementById(modalId));
    
    // Handle confirm button click
    $(`#${modalId} .confirm-btn`).click(function() {
        modal.hide();
        if (onConfirm) onConfirm();
    });
    
    // Handle modal hidden event
    $(`#${modalId}`).on('hidden.bs.modal', function() {
        $(this).remove();
        if (onCancel) onCancel();
    });
    
    modal.show();
}

// Date formatting utilities
function formatDate(dateString, format = 'dd/MM/yyyy') {
    if (!dateString) return '';
    
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return '';
    
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hour = String(date.getHours()).padStart(2, '0');
    const minute = String(date.getMinutes()).padStart(2, '0');
    
    switch (format) {
        case 'dd/MM/yyyy': return `${day}/${month}/${year}`;
        case 'dd/MM/yyyy HH:mm': return `${day}/${month}/${year} ${hour}:${minute}`;
        case 'yyyy-MM-dd': return `${year}-${month}-${day}`;
        default: return `${day}/${month}/${year}`;
    }
}

function formatRelativeTime(dateString) {
    if (!dateString) return '';
    
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);
    
    if (diffMins < 1) return 'Vừa xong';
    if (diffMins < 60) return `${diffMins} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays < 7) return `${diffDays} ngày trước`;
    
    return formatDate(dateString);
}

// API helper functions
function callApi(endpoint, method = 'GET', data = null, options = {}) {
    const token = getAuthToken();
    
    const defaultOptions = {
        url: `${API_BASE_URL}${endpoint}`,
        type: method,
        contentType: 'application/json',
        headers: {
            'Authorization': token ? `Bearer ${token}` : ''
        },
        ...options
    };
    
    if (data && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
        defaultOptions.data = JSON.stringify(data);
    }
    
    return $.ajax(defaultOptions);
}

function getAuthToken() {
    // This should match how you store the token in your application
    // For now, we'll try to get it from a data attribute or session
    return $('meta[name="auth-token"]').attr('content') || '';
}

// Form validation helpers
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function validatePhone(phone) {
    const phoneRegex = /^[\+]?[0-9\-\s\(\)]{10,15}$/;
    return phoneRegex.test(phone);
}

function validateRequired(value) {
    return value && value.toString().trim().length > 0;
}

function showFieldError(fieldName, message) {
    const field = $(`#${fieldName}, [name="${fieldName}"]`);
    field.addClass('is-invalid');
    field.siblings('.invalid-feedback').text(message);
    
    // If no invalid-feedback element exists, create one
    if (field.siblings('.invalid-feedback').length === 0) {
        field.after(`<div class="invalid-feedback">${message}</div>`);
    }
}

function clearFieldError(fieldName) {
    const field = $(`#${fieldName}, [name="${fieldName}"]`);
    field.removeClass('is-invalid');
    field.siblings('.invalid-feedback').text('');
}

function clearAllErrors(formSelector = 'form') {
    $(formSelector).find('.is-invalid').removeClass('is-invalid');
    $(formSelector).find('.invalid-feedback').text('');
}

// Table utilities
function initDataTable(tableSelector, options = {}) {
    const defaultOptions = {
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        language: {
            "lengthMenu": "Hiển thị _MENU_ dòng mỗi trang",
            "zeroRecords": "Không tìm thấy dữ liệu",
            "info": "Hiển thị _START_ đến _END_ của _TOTAL_ dòng",
            "infoEmpty": "Hiển thị 0 đến 0 của 0 dòng",
            "infoFiltered": "(lọc từ _MAX_ dòng)",
            "search": "Tìm kiếm:",
            "paginate": {
                "first": "Đầu",
                "last": "Cuối",
                "next": "Tiếp",
                "previous": "Trước"
            }
        },
        ...options
    };
    
    return $(tableSelector).DataTable(defaultOptions);
}

// Role-based UI utilities
function hideElementsByRole(userRole) {
    // Hide elements based on user role
    $('[data-roles]').each(function() {
        const allowedRoles = $(this).data('roles').split(',');
        if (!allowedRoles.includes(userRole)) {
            $(this).hide();
        }
    });
}

function showElementsByRole(userRole) {
    // Show elements based on user role
    $('[data-roles]').each(function() {
        const allowedRoles = $(this).data('roles').split(',');
        if (allowedRoles.includes(userRole)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

// Local storage utilities
function setLocalStorage(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
    } catch (e) {
        console.warn('LocalStorage not available:', e);
    }
}

function getLocalStorage(key, defaultValue = null) {
    try {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : defaultValue;
    } catch (e) {
        console.warn('LocalStorage not available:', e);
        return defaultValue;
    }
}

function removeLocalStorage(key) {
    try {
        localStorage.removeItem(key);
    } catch (e) {
        console.warn('LocalStorage not available:', e);
    }
}

// Initialize on document ready
$(document).ready(function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
    
    // Auto-dismiss alerts after 5 seconds
    setTimeout(function() {
        $('.alert:not(.alert-permanent)').fadeOut();
    }, 5000);
    
    // Handle AJAX form submissions with data-ajax="true"
    $('form[data-ajax="true"]').on('submit', function(e) {
        e.preventDefault();
        
        const form = $(this);
        const url = form.attr('action');
        const method = form.attr('method') || 'POST';
        const formData = new FormData(this);
        
        $.ajax({
            url: url,
            type: method,
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                if (response.success) {
                    showNotification(response.message || 'Thao tác thành công', 'success');
                    
                    // Trigger custom event
                    form.trigger('ajax:success', [response]);
                } else {
                    showNotification(response.message || 'Có lỗi xảy ra', 'error');
                }
            },
            error: function(xhr) {
                const message = xhr.responseJSON?.message || 'Có lỗi xảy ra khi xử lý yêu cầu';
                showNotification(message, 'error');
            }
        });
    });
    
    // Global error handler for AJAX requests
    $(document).ajaxError(function(event, xhr, settings, thrownError) {
        if (xhr.status === 401) {
            showNotification('Phiên đăng nhập đã hết hạn. Đang chuyển hướng...', 'warning');
            setTimeout(() => {
                window.location.href = '/Auth/Login';
            }, 2000);
        } else if (xhr.status === 403) {
            showNotification('Bạn không có quyền thực hiện thao tác này.', 'error');
        } else if (xhr.status === 500) {
            showNotification('Lỗi hệ thống. Vui lòng thử lại sau.', 'error');
        }
    });
});