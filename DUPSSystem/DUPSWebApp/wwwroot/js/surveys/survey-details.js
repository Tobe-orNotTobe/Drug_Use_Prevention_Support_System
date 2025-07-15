// survey-details.js - Chi tiết khảo sát
const API_BASE_URL = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || 'https://localhost:7008';

function getAuthToken() {
    return document.querySelector('meta[name="auth-token"]')?.getAttribute('content') || '';
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

function loadSurveyDetails(surveyId) {
    fetch(`${API_BASE_URL}/odata/Surveys(${surveyId})?$expand=Questions($expand=Options)`, {
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
            displaySurveyDetails(survey);
        })
        .catch(error => {
            console.error('Error loading survey details:', error);
            showAlert('Không thể tải chi tiết khảo sát', 'danger');
        });
}

function displaySurveyDetails(survey) {
    document.getElementById('surveyName').textContent = survey.Name;
    document.getElementById('surveyDescription').textContent = survey.Description || 'Không có mô tả';
    document.getElementById('targetAudiences').textContent = survey.TargetAudiences;

    const statusClass = survey.Status === 'Active' ? 'bg-success' : 'bg-secondary';
    const statusText = survey.Status === 'Active' ? 'Hoạt động' : 'Không hoạt động';
    const statusBadge = document.getElementById('surveyStatus');
    statusBadge.className = `badge ${statusClass}`;
    statusBadge.textContent = statusText;

    document.getElementById('CreatedAt').textContent = formatDate(survey.CreatedAt);

    document.getElementById('questionCount').textContent = `${survey.Questions?.length || 0} câu hỏi`;

    displayQuestions(survey.Questions || []);

    // Load statistics if user is Staff/Admin
    const userRole = document.body.dataset.userRole;
    if (userRole === 'Staff' || userRole === 'Admin') {
        loadSurveyStatistics(survey.SurveyId);
    }

    document.getElementById('loadingSpinner').style.display = 'none';
    document.getElementById('surveyDetailsContainer').style.display = 'block';
}

function displayQuestions(questions) {
    const container = document.getElementById('questionsContainer');
    container.innerHTML = '';

    if (questions.length === 0) {
        container.innerHTML = `
            <div class="alert alert-info text-center">
                <i class="fas fa-info-circle"></i>
                Khảo sát này chưa có câu hỏi nào
            </div>
        `;
        return;
    }

    questions.forEach((question, index) => {
        const questionHtml = createQuestionPreview(question, index + 1);
        container.insertAdjacentHTML('beforeend', questionHtml);
    });
}

function createQuestionPreview(question, questionNumber) {
    const typeText = {
        'SingleChoice': 'Chọn một đáp án',
        'MultipleChoice': 'Chọn nhiều đáp án',
        'Text': 'Câu trả lời tự do'
    };

    const typeIcon = {
        'SingleChoice': 'fas fa-dot-circle',
        'MultipleChoice': 'fas fa-check-square',
        'Text': 'fas fa-edit'
    };

    let optionsHtml = '';
    if (question.Type !== 'Text' && question.Options && question.Options.length > 0) {
        const optionsList = question.Options.map((option, index) => {
            const inputType = question.Type === 'SingleChoice' ? 'radio' : 'checkbox';
            return `
                <div class="form-check">
                    <input class="form-check-input" type="${inputType}" disabled>
                    <label class="form-check-label text-muted">
                        ${option.Content}
                    </label>
                </div>
            `;
        }).join('');
        optionsHtml = `<div class="mt-3">${optionsList}</div>`;
    } else if (question.Type === 'Text') {
        optionsHtml = `
            <div class="mt-3">
                <textarea class="form-control" placeholder="Người dùng sẽ nhập câu trả lời ở đây..." disabled rows="3"></textarea>
            </div>
        `;
    }

    return `
        <div class="card mb-3">
            <div class="card-header">
                <div class="d-flex justify-content-between align-items-center">
                    <span class="fw-bold">
                        <span class="badge bg-primary me-2">${questionNumber}</span>
                        ${question.Content}
                    </span>
                    <span class="badge bg-secondary">
                        <i class="${typeIcon[question.Type]}"></i>
                        ${typeText[question.Type]}
                    </span>
                </div>
            </div>
            <div class="card-body">
                ${optionsHtml}
            </div>
        </div>
    `;
}

function loadSurveyStatistics(surveyId) {
    fetch(`${API_BASE_URL}/api/Reports/SurveyStats/${surveyId}`, {
        method: 'GET',
        headers: getApiHeaders()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(stats => {
            displaySurveyStatistics(stats);
        })
        .catch(error => {
            console.error('Error loading survey statistics:', error);
            // Don't show error alert for statistics as it's not critical
        });
}

function displaySurveyStatistics(stats) {
    document.getElementById('totalResponses').textContent = stats.totalResponses || 0;
    document.getElementById('completedResponses').textContent = stats.completedResponses || 0;
    document.getElementById('averageTime').textContent = stats.averageTime || 0;
    document.getElementById('responseRate').textContent = (stats.responseRate || 0) + '%';
}

function formatDate(dateString) {
    if (!dateString) return 'Không có thông tin';

    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function takeSurvey(surveyId) {
    window.location.href = `/Surveys/Take/${surveyId}`;
}

function viewReport(surveyId) {
    window.location.href = `/Surveys/Result/${surveyId}`;
}

// Event listeners
document.addEventListener('DOMContentLoaded', function () {
    if (typeof SURVEY_ID !== 'undefined') {
        loadSurveyDetails(SURVEY_ID);
    }

    // Take survey button
    const takeSurveyBtn = document.getElementById('takeSurveyBtn');
    if (takeSurveyBtn) {
        takeSurveyBtn.addEventListener('click', function () {
            const surveyId = this.dataset.surveyId;
            takeSurvey(surveyId);
        });
    }

    // View report button
    const viewReportBtn = document.getElementById('viewReportBtn');
    if (viewReportBtn) {
        viewReportBtn.addEventListener('click', function () {
            const surveyId = this.dataset.surveyId;
            viewReport(surveyId);
        });
    }
});