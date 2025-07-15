const API_BASE_URL = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || 'https://localhost:7008';

let selectedSurveyId = null;

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

function loadAvailableSurveys() {
    document.getElementById('loadingSpinner').classList.remove('d-none');

    // Gọi OData để lấy surveys với expand SurveyQuestions để count số câu hỏi
    fetch(`${API_BASE_URL}/odata/Surveys?$expand=SurveyQuestions&$orderby=CreatedAt desc`, {
        method: 'GET',
        headers: getApiHeaders()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            displayAvailableSurveys(data.value || data);
            document.getElementById('loadingSpinner').classList.add('d-none');
        })
        .catch(error => {
            console.error('Error loading available surveys:', error);
            showAlert('Không thể tải danh sách khảo sát', 'danger');
            document.getElementById('loadingSpinner').classList.add('d-none');
        });
}

function displayAvailableSurveys(surveys) {
    const container = document.getElementById('surveysContainer');
    container.innerHTML = '';

    if (!surveys || surveys.length === 0) {
        document.getElementById('noSurveys').classList.remove('d-none');
        return;
    }

    surveys.forEach(survey => {
        const questionCount = survey.SurveyQuestions ? survey.SurveyQuestions.length : 0;
        const estimatedTime = Math.max(5, questionCount * 2); // Ước tính 2 phút/câu hỏi, tối thiểu 5 phút

        const cardHtml = `
            <div class="col-md-6 col-lg-4 mb-4">
                <div class="card h-100 survey-card" style="cursor: pointer; transition: all 0.3s;" onclick="showSurveyInfo(${survey.SurveyId})">
                    <div class="card-body">
                        <h5 class="card-title text-primary">${survey.Name}</h5>
                        <p class="card-text text-muted">${survey.Description || 'Không có mô tả'}</p>
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <small class="text-muted">
                                <i class="fas fa-question-circle"></i> ${questionCount} câu hỏi
                            </small>
                            <small class="text-muted">
                                <i class="fas fa-clock"></i> ~${estimatedTime} phút
                            </small>
                        </div>
                        <div class="mb-2">
                            <small class="text-muted">
                                <i class="fas fa-calendar"></i> ${formatDate(survey.CreatedAt)}
                            </small>
                        </div>
                    </div>
                    <div class="card-footer bg-transparent">
                        <div class="d-grid">
                            <button class="btn btn-primary" onclick="event.stopPropagation(); showSurveyInfo(${survey.SurveyId})">
                                <i class="fas fa-play"></i> Bắt đầu khảo sát
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', cardHtml);
    });

    // Add hover effects
    document.querySelectorAll('.survey-card').forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.classList.add('shadow-lg');
            this.style.transform = 'translateY(-5px)';
        });

        card.addEventListener('mouseleave', function () {
            this.classList.remove('shadow-lg');
            this.style.transform = 'translateY(0)';
        });
    });
}

function showSurveyInfo(surveyId) {
    selectedSurveyId = surveyId;
    getSurveyDetails(surveyId);
}

function getSurveyDetails(surveyId) {
    // Gọi OData với expand SurveyQuestions để lấy chi tiết
    fetch(`${API_BASE_URL}/odata/Surveys(${surveyId})?$expand=SurveyQuestions`, {
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
            displaySurveyModal(survey);
        })
        .catch(error => {
            console.error('Error loading survey details:', error);
            showAlert('Không thể tải thông tin khảo sát', 'danger');
        });
}

function displaySurveyModal(survey) {
    const modal = document.getElementById('surveyInfoModal');
    const title = document.getElementById('surveyModalTitle');
    const body = document.getElementById('surveyModalBody');

    title.textContent = survey.Name;

    const questionCount = survey.SurveyQuestions ? survey.SurveyQuestions.length : 0;
    const estimatedTime = Math.max(5, questionCount * 2);

    body.innerHTML = `
        <div class="mb-3">
            <h6><i class="fas fa-info-circle"></i> Mô tả:</h6>
            <p>${survey.Description || 'Không có mô tả'}</p>
        </div>
        <div class="row mb-3">
            <div class="col-md-6">
                <h6><i class="fas fa-question-circle"></i> Số câu hỏi:</h6>
                <span class="badge bg-info">${questionCount} câu hỏi</span>
            </div>
            <div class="col-md-6">
                <h6><i class="fas fa-clock"></i> Thời gian ước tính:</h6>
                <span class="badge bg-warning">${estimatedTime} phút</span>
            </div>
        </div>
        <div class="mb-3">
            <h6><i class="fas fa-calendar"></i> Ngày tạo:</h6>
            <span class="text-muted">${formatDate(survey.CreatedAt)}</span>
        </div>
        <div class="alert alert-info">
            <i class="fas fa-lightbulb"></i> 
            <strong>Lợi ích:</strong> Khảo sát này sẽ giúp đánh giá mức độ rủi ro và đưa ra những khuyến nghị phù hợp để hỗ trợ bạn.
        </div>
        <div class="alert alert-warning">
            <i class="fas fa-lock"></i> 
            <strong>Bảo mật:</strong> Thông tin của bạn sẽ được bảo mật tuyệt đối và chỉ được sử dụng cho mục đích hỗ trợ và cải thiện dịch vụ.
        </div>
    `;

    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();
}

function startSurvey() {
    if (selectedSurveyId) {
        // Kiểm tra xem user đã làm khảo sát này chưa
        checkSurveyStatus(selectedSurveyId);
    }
}

function checkSurveyStatus(surveyId) {
    const userId = document.querySelector('meta[name="current-user-id"]')?.getAttribute('content');

    if (!userId) {
        showAlert('Không thể xác định thông tin người dùng', 'danger');
        return;
    }

    fetch(`${API_BASE_URL}/api/SurveyResults/check/${userId}/${surveyId}`, {
        method: 'GET',
        headers: getApiHeaders()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(result => {
            if (result.success && result.data.hasTaken) {
                if (confirm('Bạn đã làm khảo sát này rồi. Bạn có muốn xem kết quả không?')) {
                    // Redirect to result page or show result
                    showAlert('Chức năng xem kết quả đang được phát triển', 'info');
                }
            } else {
                // Chuyển đến trang làm khảo sát
                window.location.href = `/Surveys/Take/${surveyId}`;
            }
        })
        .catch(error => {
            console.error('Error checking survey status:', error);
            // Nếu lỗi kiểm tra, vẫn cho phép làm khảo sát
            window.location.href = `/Surveys/Take/${surveyId}`;
        });
}

function formatDate(dateString) {
    if (!dateString) return 'Không có thông tin';

    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

// Event listeners
document.addEventListener('DOMContentLoaded', function () {
    loadAvailableSurveys();

    // Start survey button
    document.getElementById('startSurveyBtn').addEventListener('click', startSurvey);
});