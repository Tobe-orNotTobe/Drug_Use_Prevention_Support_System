$(document).ready(function () {

    // ==============================
    // TOGGLE PASSWORD VISIBILITY
    // ==============================
    $('#togglePassword, #toggleConfirmPassword').click(function () {
        const targetId = $(this).attr('id') === 'togglePassword' ? '#password' : '#confirmPassword';
        const passwordInput = $(targetId);
        const icon = $(this).find('i');

        if (passwordInput.attr('type') === 'password') {
            passwordInput.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordInput.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // ==============================
    // LOGIN SUBMIT
    // ==============================
    $('#loginForm').submit(function (e) {
        e.preventDefault();

        $('.form-control').removeClass('is-invalid');
        $('.invalid-feedback').text('');

        const formData = {
            email: $('#email').val(),
            password: $('#password').val()
        };

        if (!validateLoginForm(formData)) {
            return;
        }

        const loginBtn = $('#loginBtn');
        const originalText = loginBtn.html();
        loginBtn.html('<i class="fas fa-spinner fa-spin"></i> Đang đăng nhập...')
            .prop('disabled', true);

        $.ajax({
            url: '/Auth/Login',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    setTimeout(() => {
                        const returnUrl = response.redirectUrl || '/';
                        window.location.href = returnUrl;
                    }, 1000);
                } else {
                    showNotification(response.message, 'error');
                    resetLoginButton();
                }
            },
            error: function (xhr) {
                let errorMessage = 'Đã xảy ra lỗi trong quá trình đăng nhập';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                showNotification(errorMessage, 'error');
                resetLoginButton();
            }
        });

        function resetLoginButton() {
            loginBtn.html(originalText).prop('disabled', false);
        }
    });

    // ==============================
    // REGISTER SUBMIT
    // ==============================
    $('#registerForm').submit(function (e) {
        e.preventDefault();

        $('.form-control, .form-select, .form-check-input').removeClass('is-invalid');
        $('.invalid-feedback').text('');

        const formData = {
            fullName: $('#fullName').val(),
            email: $('#email').val(),
            password: $('#password').val(),
            confirmPassword: $('#confirmPassword').val(),
            phone: $('#phone').val(),
            dateOfBirth: $('#dateOfBirth').val() || null,
            gender: $('#gender').val() || null,
            address: $('#address').val() || null
        };

        if (!validateRegisterForm(formData)) {
            return;
        }

        const registerBtn = $('#registerBtn');
        const originalText = registerBtn.html();
        registerBtn.html('<i class="fas fa-spinner fa-spin"></i> Đang đăng ký...')
            .prop('disabled', true);

        $.ajax({
            url: '/Auth/Register',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    showNotification(response.message, 'success');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 2000);
                } else {
                    showNotification(response.message, 'error');
                    resetRegisterButton();
                }
            },
            error: function (xhr) {
                let errorMessage = 'Đã xảy ra lỗi trong quá trình đăng ký';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }
                showNotification(errorMessage, 'error');
                resetRegisterButton();
            }
        });

        function resetRegisterButton() {
            registerBtn.html(originalText).prop('disabled', false);
        }
    });

    // ==============================
    // PROFILE & CHANGE PASSWORD
    // ==============================

    // Submit profile form
    $('#profileForm').submit(function (e) {
        e.preventDefault();
        updateProfile();
    });

    // Submit change password form
    $('#changePasswordForm').submit(function (e) {
        e.preventDefault();
        changePassword();
    });

    // Real-time confirm password match
    $('#confirmPassword').on('input', function () {
        const newPassword = $('#newPassword').val();
        const confirmPassword = $(this).val();

        if (confirmPassword && newPassword !== confirmPassword) {
            $(this).addClass('is-invalid');
            $(this).siblings('.invalid-feedback').text('Mật khẩu xác nhận không khớp');
        } else {
            $(this).removeClass('is-invalid');
            $(this).siblings('.invalid-feedback').text('');
        }
    });

    // Max date for DOB
    if ($('#dateOfBirth').length > 0) {
        const today = new Date();
        const maxDate = new Date(today.getFullYear() - 18, today.getMonth(), today.getDate());
        $('#dateOfBirth').attr('max', maxDate.toISOString().split('T')[0]);
    }

    // Focus fields
    if ($('#email').length > 0) {
        $('#email').focus();
    }
    if ($('#fullName').length > 0) {
        $('#fullName').focus();
    }
});

// ==============================
// PROFILE FUNCTIONS
// ==============================

function updateProfile() {
    const formData = {
        fullName: $('#fullName').val(),
        phone: $('#phone').val(),
        address: $('#address').val(),
        dateOfBirth: $('#dateOfBirth').val() || null,
        gender: $('#gender').val() || null
    };

    if (!validateProfileForm(formData)) {
        return;
    }

    const submitBtn = $('#profileForm button[type="submit"]');
    const originalText = submitBtn.html();
    submitBtn.html('<i class="fas fa-spinner fa-spin"></i> Đang lưu...').prop('disabled', true);

    $.ajax({
        url: '/Account/UpdateProfile',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (response) {
            if (response.success) {
                showNotification(response.message, 'success');

                if (formData.fullName) {
                    $('.navbar-nav .nav-link:contains("@ViewBag.CurrentUserName")').text(formData.fullName);
                }
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function (xhr) {
            const message = xhr.responseJSON?.message || 'Có lỗi xảy ra khi cập nhật hồ sơ';
            showNotification(message, 'error');
        },
        complete: function () {
            submitBtn.html(originalText).prop('disabled', false);
        }
    });
}

function changePassword() {
    const formData = {
        currentPassword: $('#currentPassword').val(),
        newPassword: $('#newPassword').val(),
        confirmPassword: $('#confirmPassword').val()
    };

    if (!validatePasswordForm(formData)) {
        return;
    }

    const submitBtn = $('#changePasswordForm button[type="submit"]');
    const originalText = submitBtn.html();
    submitBtn.html('<i class="fas fa-spinner fa-spin"></i> Đang đổi...').prop('disabled', true);

    $.ajax({
        url: '/Account/ChangePassword',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(formData),
        success: function (response) {
            if (response.success) {
                showNotification(response.message, 'success');
                $('#changePasswordForm')[0].reset();
                clearAllErrors('#changePasswordForm');
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function (xhr) {
            const message = xhr.responseJSON?.message || 'Có lỗi xảy ra khi đổi mật khẩu';
            showNotification(message, 'error');
        },
        complete: function () {
            submitBtn.html(originalText).prop('disabled', false);
        }
    });
}

function validateLoginForm(data) {
    let isValid = true;

    if (!data.email) {
        showFieldError('email', 'Email là bắt buộc');
        isValid = false;
    } else if (!isValidEmail(data.email)) {
        showFieldError('email', 'Email không hợp lệ');
        isValid = false;
    }

    if (!data.password) {
        showFieldError('password', 'Mật khẩu là bắt buộc');
        isValid = false;
    } else if (data.password.length < 6) {
        showFieldError('password', 'Mật khẩu phải có ít nhất 6 ký tự');
        isValid = false;
    }

    return isValid;

}

function validateRegisterForm(data) {
    let isValid = true;

    if (!data.fullName || data.fullName.trim().length < 2) {
        showFieldError('fullName', 'Họ tên phải có ít nhất 2 ký tự');
        isValid = false;
    }

    if (!data.email) {
        showFieldError('email', 'Email là bắt buộc');
        isValid = false;
    } else if (!isValidEmail(data.email)) {
        showFieldError('email', 'Email không hợp lệ');
        isValid = false;
    }

    if (!data.password) {
        showFieldError('password', 'Mật khẩu là bắt buộc');
        isValid = false;
    } else if (data.password.length < 6) {
        showFieldError('password', 'Mật khẩu phải có ít nhất 6 ký tự');
        isValid = false;
    }

    if (!data.confirmPassword) {
        showFieldError('confirmPassword', 'Xác nhận mật khẩu là bắt buộc');
        isValid = false;
    } else if (data.password !== data.confirmPassword) {
        showFieldError('confirmPassword', 'Mật khẩu xác nhận không khớp');
        isValid = false;
    }

    if (data.phone && !isValidPhone(data.phone)) {
        showFieldError('phone', 'Số điện thoại không hợp lệ');
        isValid = false;
    }

    if (!$('#agreeTerms').is(':checked')) {
        showFieldError('agreeTerms', 'Bạn phải đồng ý với điều khoản sử dụng');
        $('#agreeTerms').addClass('is-invalid');
        isValid = false;
    }

    return isValid;
}

function validateProfileForm(data) {
    let isValid = true;
    clearAllErrors('#profileForm');

    if (!data.fullName || data.fullName.trim().length < 2) {
        showFieldError('fullName', 'Họ tên phải có ít nhất 2 ký tự');
        isValid = false;
    }

    if (data.phone && !isValidPhone(data.phone)) {
        showFieldError('phone', 'Số điện thoại không hợp lệ');
        isValid = false;
    }

    return isValid;
}

function validatePasswordForm(data) {
    let isValid = true;
    clearAllErrors('#changePasswordForm');

    if (!data.currentPassword) {
        showFieldError('currentPassword', 'Mật khẩu hiện tại là bắt buộc');
        isValid = false;
    }

    if (!data.newPassword) {
        showFieldError('newPassword', 'Mật khẩu mới là bắt buộc');
        isValid = false;
    } else if (data.newPassword.length < 6) {
        showFieldError('newPassword', 'Mật khẩu mới phải có ít nhất 6 ký tự');
        isValid = false;
    }

    if (!data.confirmPassword) {
        showFieldError('confirmPassword', 'Xác nhận mật khẩu là bắt buộc');
        isValid = false;
    } else if (data.newPassword !== data.confirmPassword) {
        showFieldError('confirmPassword', 'Mật khẩu xác nhận không khớp');
        isValid = false;
    }

    return isValid;
}

function showFieldError(fieldName, message) {
    $(`#${fieldName}`).addClass('is-invalid');
    $(`#${fieldName}`).siblings('.invalid-feedback').text(message);
}

function clearAllErrors(formSelector) {
    $(formSelector).find('.is-invalid').removeClass('is-invalid');
    $(formSelector).find('.invalid-feedback').text('');
}

function isValidEmail(email) {
    const emailRegex = new RegExp("^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$");
    return emailRegex.test(email);
}

function isValidPhone(phone) {
    const phoneRegex = /^[\+]?[0-9\-\s\(\)]{10,15}$/;
    return phoneRegex.test(phone);
}

function showNotification(message, type) {
    alert(`${type}: ${message}`);
}
