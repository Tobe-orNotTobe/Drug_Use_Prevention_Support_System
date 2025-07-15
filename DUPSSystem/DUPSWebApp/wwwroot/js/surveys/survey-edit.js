// survey-edit.js - Chỉnh sửa khảo sát
const API_BASE_URL = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || 'https://localhost:7008';

let questionCounter = 0;
let editingQuestionIndex = -1;
let existingQuestions = [];

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

function loadSurveyData() {
    const surveyId = document.getElementById('surveyId').value;
    
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
        populateSurveyForm(survey);
    })
    .catch(error => {
        console.error('Error loading survey for edit:', error);
        showAlert('Không thể tải thông tin khảo sát', 'danger');
    });
}

function populateSurveyForm(survey) {
    document.getElementById('surveyName').value = survey.Name || '';
    document.getElementById('description').value = survey.Description || '';
    document.getElementById('targetAudiences').value = survey.TargetAudiences || '';
    document.getElementById('status').value = survey.Status || 'Active';
    
    existingQuestions = survey.Questions || [];
    renderExistingQuestions();
    
    document.getElementById('loadingSpinner').style.display = 'none';
    document.getElementById('editSurveyForm').style.display = 'block';
}

function renderExistingQuestions() {
    document.getElementById('questionsContainer').innerHTML = '';
    questionCounter = 0;
    
    existingQuestions.forEach((question, index) => {
        createQuestionCard(
            question.Content, 
            question.Type, 
            question.Options || [], 
            question.QuestionId
        );
    });
}

function addQuestion() {
    editingQuestionIndex = -1;
    resetQuestionForm();
    const modal = new bootstrap.Modal(document.getElementById('questionModal'));
    modal.show();
}

function editQuestion(index) {
    editingQuestionIndex = index;
    const questionCard = document.querySelector(`.question-card[data-index="${index}"]`);
    
    document.getElementById('questionContent').value = questionCard.querySelector('.question-content').textContent;
    document.getElementById('questionType').value = questionCard.dataset.type;
    document.getElementById('questionId').value = questionCard.dataset.id || '';
    
    toggleOptionsSection();
    
    const optionsContainer = document.getElementById('optionsContainer');
    optionsContainer.innerHTML = '';
    
    questionCard.querySelectorAll('.option-item').forEach(option => {
        addOption(option.textContent);
    });
    
    const modal = new bootstrap.Modal(document.getElementById('questionModal'));
    modal.show();
}

function removeQuestion(index) {
    if (confirm('Bạn có chắc chắn muốn xóa câu hỏi này?')) {
        const questionCard = document.querySelector(`.question-card[data-index="${index}"]`);
        if (questionCard) {
            questionCard.remove();
        }
    }
}

function toggleOptionsSection() {
    const questionType = document.getElementById('questionType').value;
    const optionsSection = document.getElementById('optionsSection');
    
    if (questionType === 'Text') {
        optionsSection.style.display = 'none';
    } else {
        optionsSection.style.display = 'block';
        const optionsContainer = document.getElementById('optionsContainer');
        if (optionsContainer.children.length === 0) {
            addOption();
            addOption();
        }
    }
}

function addOption(text = '') {
    const optionHtml = `
        <div class="input-group mb-2 option-input">
            <input type="text" class="form-control" placeholder="Nhập lựa chọn..." value="${text}">
            <button type="button" class="btn btn-outline-danger" onclick="removeOption(this)">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    document.getElementById('optionsContainer').insertAdjacentHTML('beforeend', optionHtml);
}

function removeOption(button) {
    button.closest('.option-input').remove();
}

function resetQuestionForm() {
    document.getElementById('questionForm').reset();
    document.getElementById('optionsContainer').innerHTML = '';
    document.getElementById('questionId').value = '';
    toggleOptionsSection();
}

function saveQuestion() {
    const content = document.getElementById('questionContent').value.trim();
    const type = document.getElementById('questionType').value;
    const questionId = document.getElementById('questionId').value;
    
    if (!content) {
        alert('Vui lòng nhập nội dung câu hỏi');
        return;
    }

    let options = [];
    if (type !== 'Text') {
        const optionInputs = document.querySelectorAll('#optionsContainer .option-input input');
        optionInputs.forEach(input => {
            const optionText = input.value.trim();
            if (optionText) {
                options.push({ Content: optionText });
            }
        });
        
        if (options.length < 2) {
            alert('Vui lòng nhập ít nhất 2 lựa chọn');
            return;
        }
    }

    if (editingQuestionIndex >= 0) {
        updateQuestionCard(editingQuestionIndex, content, type, options, questionId);
    } else {
        createQuestionCard(content, type, options);
    }

    const modal = bootstrap.Modal.getInstance(document.getElementById('questionModal'));
    modal.hide();
}

function createQuestionCard(content, type, options, questionId = null) {
    const typeText = {
        'SingleChoice': 'Chọn một đáp án',
        'MultipleChoice': 'Chọn nhiều đáp án',
        'Text': 'Câu trả lời tự do'
    };

    let optionsHtml = '';
    if (type !== 'Text' && options.length > 0) {
        const optionsList = options.map(option => 
            `<li class="option-item">${typeof option === 'string' ? option : option.Content}</li>`
        ).join('');
        optionsHtml = `<ul class="mt-2">${optionsList}</ul>`;
    }

    const questionHtml = `
        <div class="card question-card mb-3" data-index="${questionCounter}" data-type="${type}" data-id="${questionId || ''}">
            <div class="card-header d-flex justify-content-between align-items-center">
                <span class="fw-bold">Câu hỏi ${questionCounter + 1}: ${typeText[type]}</span>
                <div>
                    <button type="button" class="btn btn-sm btn-outline-primary" onclick="editQuestion(${questionCounter})">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeQuestion(${questionCounter})">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
            <div class="card-body">
                <p class="question-content">${content}</p>
                ${optionsHtml}
            </div>
        </div>
    `;

    document.getElementById('questionsContainer').insertAdjacentHTML('beforeend', questionHtml);
    questionCounter++;
}

function updateQuestionCard(index, content, type, options, questionId) {
    const typeText = {
        'SingleChoice': 'Chọn một đáp án',
        'MultipleChoice': 'Chọn nhiều đáp án',
        'Text': 'Câu trả lời tự do'
    };

    let optionsHtml = '';
    if (type !== 'Text' && options.length > 0) {
        const optionsList = options.map(option => 
            `<li class="option-item">${option.Content}</li>`
        ).join('');
        optionsHtml = `<ul class="mt-2">${optionsList}</ul>`;
    }

    const questionCard = document.querySelector(`.question-card[data-index="${index}"]`);
    questionCard.dataset.type = type;
    questionCard.dataset.id = questionId || '';
    questionCard.querySelector('.card-header span').textContent = `Câu hỏi ${index + 1}: ${typeText[type]}`;
    questionCard.querySelector('.question-content').textContent = content;
    
    const existingUl = questionCard.querySelector('ul');
    if (existingUl) {
        existingUl.remove();
    }
    
    if (optionsHtml) {
        questionCard.querySelector('.card-body').insertAdjacentHTML('beforeend', optionsHtml);
    }
}

function updateSurvey() {
    const surveyData = {
        SurveyId: parseInt(document.getElementById('surveyId').value),
        Name: document.getElementById('surveyName').value.trim(),
        Description: document.getElementById('description').value.trim(),
        TargetAudiences: document.getElementById('targetAudiences').value,
        Status: document.getElementById('status').value,
        Questions: []
    };

    if (!surveyData.Name) {
        alert('Vui lòng nhập tên khảo sát');
        return;
    }

    if (!surveyData.TargetAudiences) {
        alert('Vui lòng chọn đối tượng');
        return;
    }

    const questionCards = document.querySelectorAll('.question-card');
    questionCards.forEach(card => {
        const questionData = {
            QuestionId: card.dataset.id ? parseInt(card.dataset.id) : null,
            Content: card.querySelector('.question-content').textContent,
            Type: card.dataset.type,
            Options: []
        };

        card.querySelectorAll('.option-item').forEach(option => {
            questionData.Options.push({
                Content: option.textContent
            });
        });

        surveyData.Questions.push(questionData);
    });

    if (surveyData.Questions.length === 0) {
        alert('Vui lòng thêm ít nhất một câu hỏi');
        return;
    }

    submitUpdateSurvey(surveyData);
}

function submitUpdateSurvey(surveyData) {
    fetch(`${API_BASE_URL}/odata/Surveys(${surveyData.SurveyId})`, {
        method: 'PUT',
        headers: getApiHeaders(),
        body: JSON.stringify(surveyData)
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(result => {
        showAlert('Cập nhật khảo sát thành công', 'success');
        setTimeout(() => {
            window.location.href = '/Surveys';
        }, 1500);
    })
    .catch(error => {
        console.error('Error updating survey:', error);
        showAlert('Không thể cập nhật khảo sát', 'danger');
    });
}

// Event listeners
document.addEventListener('DOMContentLoaded', function() {
    loadSurveyData();
    
    // Form submit
    document.getElementById('editSurveyForm').addEventListener('submit', function(e) {
        e.preventDefault();
        updateSurvey();
    });

    // Add question button
    document.getElementById('addQuestionBtn').addEventListener('click', addQuestion);

    // Save question button
    document.getElementById('saveQuestionBtn').addEventListener('click', saveQuestion);

    // Add option button
    document.getElementById('addOptionBtn').addEventListener('click', () => addOption());

    // Question type change
    document.getElementById('questionType').addEventListener('change', toggleOptionsSection);

    // Cancel button
    document.getElementById('cancelBtn').addEventListener('click', function() {
        if (confirm('Bạn có chắc chắn muốn hủy? Các thay đổi chưa lưu sẽ bị mất.')) {
            window.location.href = '/Surveys';
        }
    });
});