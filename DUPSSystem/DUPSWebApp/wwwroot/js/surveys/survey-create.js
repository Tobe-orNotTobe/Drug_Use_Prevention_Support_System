const API_BASE_URL = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || 'https://localhost:7008';

let questionCounter = 0;
let editingQuestionIndex = -1;

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
    toggleOptionsSection();
}

function saveQuestion() {
    const content = document.getElementById('questionContent').value.trim();
    const type = document.getElementById('questionType').value;

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
                options.push(optionText);
            }
        });

        if (options.length < 2) {
            alert('Vui lòng nhập ít nhất 2 lựa chọn');
            return;
        }
    }

    if (editingQuestionIndex >= 0) {
        updateQuestionCard(editingQuestionIndex, content, type, options);
    } else {
        createQuestionCard(content, type, options);
    }

    const modal = bootstrap.Modal.getInstance(document.getElementById('questionModal'));
    modal.hide();
}

function createQuestionCard(content, type, options) {
    const typeText = {
        'SingleChoice': 'Chọn một đáp án',
        'MultipleChoice': 'Chọn nhiều đáp án',
        'Text': 'Câu trả lời tự do'
    };

    let optionsHtml = '';
    if (type !== 'Text') {
        optionsHtml = options.map(option => `<li class="option-item">${option}</li>`).join('');
        optionsHtml = `<ul class="mt-2">${optionsHtml}</ul>`;
    }

    const questionHtml = `
        <div class="card question-card mb-3" data-index="${questionCounter}" data-type="${type}">
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

    const questionsContainer = document.getElementById('questionsContainer');
    const alertInfo = questionsContainer.querySelector('.alert');
    if (alertInfo) {
        alertInfo.remove();
    }

    questionsContainer.insertAdjacentHTML('beforeend', questionHtml);
    questionCounter++;
}

function updateQuestionCard(index, content, type, options) {
    const typeText = {
        'SingleChoice': 'Chọn một đáp án',
        'MultipleChoice': 'Chọn nhiều đáp án',
        'Text': 'Câu trả lời tự do'
    };

    let optionsHtml = '';
    if (type !== 'Text') {
        optionsHtml = options.map(option => `<li class="option-item">${option}</li>`).join('');
        optionsHtml = `<ul class="mt-2">${optionsHtml}</ul>`;
    }

    const questionCard = document.querySelector(`.question-card[data-index="${index}"]`);
    questionCard.dataset.type = type;
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

function createSurvey() {
    const surveyData = {
        Name: document.getElementById('surveyName').value.trim(),
        Description: document.getElementById('description').value.trim(),
        SurveyQuestions: []
    };

    if (!surveyData.Name) {
        alert('Vui lòng nhập tên khảo sát');
        return;
    }

    const questionCards = document.querySelectorAll('.question-card');
    questionCards.forEach(card => {
        const questionData = {
            QuestionText: card.querySelector('.question-content').textContent,
            QuestionType: card.dataset.type,
            SurveyOptions: []
        };

        card.querySelectorAll('.option-item').forEach(option => {
            questionData.SurveyOptions.push({
                OptionText: option.textContent
            });
        });

        surveyData.SurveyQuestions.push(questionData);
    });

    if (surveyData.SurveyQuestions.length === 0) {
        alert('Vui lòng thêm ít nhất một câu hỏi');
        return;
    }

    submitCreateSurvey(surveyData);
}

function submitCreateSurvey(surveyData) {
    fetch(`${API_BASE_URL}/odata/Surveys`, {
        method: 'POST',
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
            showAlert('Tạo khảo sát thành công', 'success');
            setTimeout(() => {
                window.location.href = '/Surveys';
            }, 1500);
        })
        .catch(error => {
            console.error('Error creating survey:', error);
            showAlert('Không thể tạo khảo sát', 'danger');
        });
}

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('createSurveyForm').addEventListener('submit', function (e) {
        e.preventDefault();
        createSurvey();
    });

    document.getElementById('addQuestionBtn').addEventListener('click', addQuestion);
    document.getElementById('saveQuestionBtn').addEventListener('click', saveQuestion);
    document.getElementById('addOptionBtn').addEventListener('click', () => addOption());
    document.getElementById('questionType').addEventListener('change', toggleOptionsSection);

    document.getElementById('cancelBtn').addEventListener('click', function () {
        if (confirm('Bạn có chắc chắn muốn hủy? Dữ liệu chưa lưu sẽ bị mất.')) {
            window.location.href = '/Surveys';
        }
    });

    toggleOptionsSection();
});