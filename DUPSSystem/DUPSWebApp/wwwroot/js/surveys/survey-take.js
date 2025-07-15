// survey-take.js - Làm bài khảo sát
const API_BASE_URL = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || 'https://localhost:7008';

let surveyData = null;
let submittedResultId = null;

function getAuthToken() {
    return document.querySelector('meta[name="auth-token"]')?.getAttribute('content') || '';
}

function getCurrentUserId() {
    return document.querySelector('meta[name="current-user-id"]')?.getAttribute('content') || '';
}

function getApiHeaders() {
    const token = getAuthToken();
    const headers = {
        'Content-Type': 'application/json'
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    return headers;
}

function showAlert(message, type = 'info') {
    const alertClass = `alert-${type}`;
    const iconClass = type === 'success' ? 'fas fa-check-circle' :
        type === 'danger' ? 'fas fa-exclamation-triangle' :
            type === 'warning' ? 'fas fa-exclamation-circle' : 'fas fa-info-circle';

    const alertHtml = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <i class="${iconClass}"></i> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    document.querySelector('main').insertAdjacentHTML('afterbegin', alertHtml);

    setTimeout(() => {
        const alert = document.querySelector('.alert');
        if (alert) alert.remove();
    }, 5000);
}

function loadSurveyForTaking() {
    const surveyId = document.getElementById('surveyId').value;

    // Gọi OData với expand để lấy questions và options
    fetch(`${API_BASE_URL}/odata/Surveys(${surveyId})?$expand=SurveyQuestions($expand=SurveyOptions)`, {
        method: 'GET',
        headers: getApiHeaders()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(survey => {
            renderSurveyForTaking(survey);
        })
        .catch(error => {
            console.error('Error loading survey for taking:', error);
            showAlert('Không thể tải khảo sát', 'danger');
        });
}

function renderSurveyForTaking(survey) {
    surveyData = survey;

    document.getElementById('surveyTitle').textContent = survey.Name;
    document.getElementById('surveyDescription').textContent = survey.Description || '';

    // Render questions từ SurveyQuestions array
    renderQuestions(survey.SurveyQuestions || []);

    document.getElementById('loadingSpinner').style.display = 'none';
    document.getElementById('surveyContainer').style.display = 'block';
}

function renderQuestions(questions) {
    const container = document.getElementById('questionsContainer');
    container.innerHTML = '';

    questions.forEach((question, index) => {
        const questionHtml = createQuestionHtml(question, index);
        container.insertAdjacentHTML('beforeend', questionHtml);
    });
}

function createQuestionHtml(question, index) {
    const questionNumber = index + 1;
    let inputHtml = '';

    switch (question.QuestionType) {
        case 'SingleChoice':
            inputHtml = (question.SurveyOptions || []).map((option, optIndex) => `
                <div class="form-check">
                    <input class="form-check-input" type="radio" name="question_${question.QuestionId}" 
                           id="q${question.QuestionId}_opt${optIndex}" value="${option.OptionId}" required>
                    <label class="form-check-label" for="q${question.QuestionId}_opt${optIndex}">
                        ${option.OptionText}
                    </label>
                </div>
            `).join('');
            break;

        case 'MultipleChoice':
            inputHtml = (question.SurveyOptions || []).map((option, optIndex) => `
                <div class="form-check">
                    <input class="form-check-input" type="checkbox" name="question_${question.QuestionId}" 
                           id="q${question.QuestionId}_opt${optIndex}" value="${option.OptionId}">
                    <label class="form-check-label" for="q${question.QuestionId}_opt${optIndex}">
                        ${option.OptionText}
                    </label>
                </div>
            `).join('');
            break;

        case 'TextInput':
        case 'Text':
            inputHtml = `
                <textarea class="form-control" name="question_${question.QuestionId}" 
                          id="q${question.QuestionId}_text" rows="4" 
                          placeholder="Nhập câu trả lời của bạn..." required></textarea>
            `;
            break;
    }

    return `
        <div class="card mb-4 question-item" data-question-id="${question.QuestionId}" data-type="${question.QuestionType}">
            <div class="card-header">
                <h6 class="mb-0">
                    <span class="badge bg-primary me-2">${questionNumber}</span>
                    ${question.QuestionText}
                    ${question.QuestionType !== 'MultipleChoice' ? '<span class="text-danger">*</span>' : ''}
                </h6>
            </div>
            <div class="card-body">
                ${inputHtml}
            </div>
        </div>
    `;
}

function validateAndShowConfirm() {
    const errors = validateSurveyAnswers();

    const errorsDiv = document.getElementById('validationErrors');
    if (errors.length > 0) {
        errorsDiv.classList.remove('d-none');
        errorsDiv.innerHTML = '<strong>Vui lòng hoàn thành:</strong><ul>' +
            errors.map(error => `<li>${error}</li>`).join('') +
            '</ul>';
    } else {
        errorsDiv.classList.add('d-none');
    }

    const modal = new bootstrap.Modal(document.getElementById('confirmSubmitModal'));
    modal.show();
}

function validateSurveyAnswers() {
    const errors = [];

    document.querySelectorAll('.question-item').forEach((questionItem, index) => {
        const questionId = questionItem.dataset.questionId;
        const questionType = questionItem.dataset.type;
        const questionNumber = index + 1;
        const questionText = questionItem.querySelector('.card-header h6').textContent.replace(/^\d+\s*/, '').replace('*', '').trim();

        switch (questionType) {
            case 'SingleChoice':
                const radioChecked = questionItem.querySelectorAll('input[type="radio"]:checked').length > 0;
                if (!radioChecked) {
                    errors.push(`Câu ${questionNumber}: ${questionText}`);
                }
                break;

            case 'TextInput':
            case 'Text':
                const textValue = questionItem.querySelector('textarea').value.trim();
                if (!textValue) {
                    errors.push(`Câu ${questionNumber}: ${questionText}`);
                }
                break;
        }
    });

    return errors;
}

function confirmSubmitSurvey() {
    console.log('confirmSubmitSurvey called');

    const errors = validateSurveyAnswers();
    if (errors.length > 0) {
        console.log('Validation errors found:', errors);
        return;
    }

    const surveySubmission = collectSurveyAnswers();

    // Debug log để kiểm tra format
    console.log('Survey submission payload:', JSON.stringify(surveySubmission, null, 2));

    // Validation cuối cùng
    if (!surveySubmission.UserId || !surveySubmission.SurveyId || !surveySubmission.Answers || surveySubmission.Answers.length === 0) {
        console.error('Invalid submission data:', surveySubmission);
        showAlert('Dữ liệu khảo sát không hợp lệ', 'danger');
        return;
    }

    submitSurveyResult(surveySubmission);
}

function collectSurveyAnswers() {
    const answers = [];

    document.querySelectorAll('.question-item').forEach(questionItem => {
        const questionId = parseInt(questionItem.dataset.questionId);
        const questionType = questionItem.dataset.type;

        switch (questionType) {
            case 'SingleChoice':
                const checkedRadio = questionItem.querySelector('input[type="radio"]:checked');
                if (checkedRadio) {
                    answers.push({
                        QuestionId: questionId,
                        OptionId: parseInt(checkedRadio.value),
                        AnswerText: null,
                        Score: 1 // Default score, có thể lấy từ option.Score nếu cần
                    });
                }
                break;

            case 'MultipleChoice':
                questionItem.querySelectorAll('input[type="checkbox"]:checked').forEach(checkbox => {
                    answers.push({
                        QuestionId: questionId,
                        OptionId: parseInt(checkbox.value),
                        AnswerText: null,
                        Score: 1
                    });
                });
                break;

            case 'TextInput':
            case 'Text':
                const textValue = questionItem.querySelector('textarea').value.trim();
                if (textValue) {
                    answers.push({
                        QuestionId: questionId,
                        OptionId: null,
                        AnswerText: textValue,
                        Score: 1 // Có thể điều chỉnh logic scoring cho text input
                    });
                }
                break;
        }
    });

    // Format đúng theo SurveySubmissionRequest DTO
    return {
        UserId: parseInt(getCurrentUserId()),
        SurveyId: parseInt(document.getElementById('surveyId').value),
        Answers: answers
    };
}

function submitSurveyResult(surveySubmission) {
    // Show loading immediately
    const submitBtn = document.getElementById('confirmSubmitBtn');
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang gửi...';
    submitBtn.disabled = true;

    // Gọi API SurveyResults/submit theo cấu trúc backend
    fetch(`${API_BASE_URL}/api/SurveyResults/submit`, {
        method: 'POST',
        headers: getApiHeaders(),
        body: JSON.stringify(surveySubmission)
    })
        .then(response => {
            // Log response để debug
            console.log('Response status:', response.status);
            console.log('Response ok:', response.ok);

            // Luôn luôn parse JSON, kể cả khi có lỗi
            return response.json().then(data => {
                return {
                    ok: response.ok,
                    status: response.status,
                    data: data
                };
            });
        })
        .then(result => {
            console.log('Full API response:', result);

            // Hide modal trước khi xử lý
            const modal = bootstrap.Modal.getInstance(document.getElementById('confirmSubmitModal'));
            if (modal) {
                modal.hide();
            }

            // Restore button
            submitBtn.innerHTML = originalText;
            submitBtn.disabled = false;

            // Check cả response.ok và result.data.Success
            if (result.ok && result.data && result.data.success) {
                console.log('Submit successful, ResultId:', result.data.ResultId);
                // Delay một chút để modal hide hoàn toàn
                setTimeout(() => {
                    showCompletedState(result.data.ResultId);
                }, 500);
            } else {
                // Có lỗi từ API
                const errorMessage = result.data?.Message || 'Không thể nộp bài khảo sát';
                console.log('Submit failed:', errorMessage);
                showAlert(errorMessage, 'danger');
            }
        })
        .catch(error => {
            console.error('Error submitting survey result:', error);

            // Hide modal nếu có lỗi
            const modal = bootstrap.Modal.getInstance(document.getElementById('confirmSubmitModal'));
            if (modal) {
                modal.hide();
            }

            // Restore button
            submitBtn.innerHTML = originalText;
            submitBtn.disabled = false;

            showAlert('Lỗi kết nối: ' + error.message, 'danger');
        });
}

function showCompletedState(resultId) {
    console.log('showCompletedState called with resultId:', resultId);

    submittedResultId = resultId;

    // Đảm bảo tất cả modal đã đóng
    const allModals = document.querySelectorAll('.modal');
    allModals.forEach(modal => {
        const modalInstance = bootstrap.Modal.getInstance(modal);
        if (modalInstance) {
            modalInstance.hide();
        }
    });

    // Force hide survey container và show completed container
    const surveyContainer = document.getElementById('surveyContainer');
    const completedContainer = document.getElementById('completedContainer');

    if (surveyContainer) {
        surveyContainer.style.display = 'none';
        console.log('Survey container hidden');
    }

    if (completedContainer) {
        completedContainer.style.display = 'block';
        console.log('Completed container shown');
    }

    // Scroll to top để user thấy thông báo
    window.scrollTo(0, 0);

    // Show success alert
    showAlert('Gửi khảo sát thành công! Cảm ơn bạn đã tham gia.', 'success');
}

function viewResult() {
    if (submittedResultId) {
        window.location.href = `/Surveys/Result/${submittedResultId}`;
    } else {
        showAlert('Không tìm thấy kết quả khảo sát', 'warning');
    }
}

// Event listeners
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM Content Loaded');

    // Kiểm tra userId có tồn tại không
    const userId = getCurrentUserId();
    console.log('Current user ID:', userId);

    if (!userId || userId === '0') {
        showAlert('Vui lòng đăng nhập để làm khảo sát', 'warning');
        setTimeout(() => {
            window.location.href = '/Auth/Login';
        }, 2000);
        return;
    }

    loadSurveyForTaking();

    // Form submit
    const surveyForm = document.getElementById('surveyForm');
    if (surveyForm) {
        surveyForm.addEventListener('submit', function (e) {
            console.log('Form submitted');
            e.preventDefault();
            validateAndShowConfirm();
        });
    }

    // Back button
    const backBtn = document.getElementById('backBtn');
    if (backBtn) {
        backBtn.addEventListener('click', function () {
            if (confirm('Bạn có chắc chắn muốn thoát? Dữ liệu chưa lưu sẽ bị mất.')) {
                window.history.back();
            }
        });
    }

    // Confirm submit button
    const confirmSubmitBtn = document.getElementById('confirmSubmitBtn');
    if (confirmSubmitBtn) {
        confirmSubmitBtn.addEventListener('click', function () {
            console.log('Confirm submit button clicked');
            confirmSubmitSurvey();
        });
    }

    // View result button
    const viewResultBtn = document.getElementById('viewResultBtn');
    if (viewResultBtn) {
        viewResultBtn.addEventListener('click', viewResult);
    }

    // Test function cho debugging
    window.testShowCompleted = function () {
        console.log('Test showing completed state');
        showCompletedState(999);
    };

    console.log('All event listeners attached');
});