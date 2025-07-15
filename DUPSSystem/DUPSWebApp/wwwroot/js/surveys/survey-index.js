// survey-index.js - Quản lý danh sách khảo sát
const API_BASE_URL = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || 'https://localhost:7008';

let currentPage = 1;
let pageSize = 10;
let searchTerm = '';
let deleteId = null;

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

function loadSurveys() {
    document.getElementById('loadingSpinner').classList.remove('d-none');
    document.getElementById('surveysTableBody').innerHTML = '';

    let url = `${API_BASE_URL}/odata/Surveys?$skip=${(currentPage - 1) * pageSize}&$top=${pageSize}&$orderby=CreatedAt desc`;

    if (searchTerm) {
        url += `&$filter=contains(tolower(Name), '${searchTerm.toLowerCase()}') or contains(tolower(TargetAudiences), '${searchTerm.toLowerCase()}')`;
    }

    fetch(url, {
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
            displaySurveys(data.value || data);
            document.getElementById('loadingSpinner').classList.add('d-none');
        })
        .catch(error => {
            console.error('Error loading surveys:', error);
            showAlert('Không thể tải danh sách khảo sát', 'danger');
            document.getElementById('loadingSpinner').classList.add('d-none');
        });
}

function displaySurveys(surveys) {
    const tbody = document.getElementById('surveysTableBody');
    tbody.innerHTML = '';

    if (!surveys || surveys.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center text-muted">
                    <i class="fas fa-inbox fa-2x mb-2"></i>
                    <br>Không có khảo sát nào
                </td>
            </tr>
        `;
        return;
    }

    surveys.forEach(survey => {
        const CreatedAt = survey.CreatedAt ? new Date(survey.CreatedAt).toLocaleDateString('vi-VN') : '';

        const row = `
            <tr>
                <td>
                    <strong>${survey.Name}</strong>
                    ${survey.Description ? `<br><small class="text-muted">${survey.Description}</small>` : ''}
                </td>
                <td>${survey.TargetAudiences}</td>
                <td>${CreatedAt}</td>
                <td>
                    <div class="btn-group" role="group">
                        <a href="/Surveys/Details/${survey.SurveyId}" class="btn btn-sm btn-outline-info" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </a>
                        <a href="/Surveys/Edit/${survey.SurveyId}" class="btn btn-sm btn-outline-warning" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </a>
                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="showDeleteModal(${survey.SurveyId}, '${survey.Name}')" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
        tbody.insertAdjacentHTML('beforeend', row);
    });
}

function searchSurveys() {
    searchTerm = document.getElementById('searchInput').value.trim();
    currentPage = 1;
    loadSurveys();
}

function showDeleteModal(id, name) {
    deleteId = id;
    const modal = document.getElementById('deleteModal');
    modal.querySelector('.modal-body p:first-of-type').textContent = `Bạn có chắc chắn muốn xóa khảo sát "${name}" không?`;

    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();
}

function confirmDelete() {
    if (deleteId) {
        deleteSurvey(deleteId);
        const modal = bootstrap.Modal.getInstance(document.getElementById('deleteModal'));
        modal.hide();
    }
}

function deleteSurvey(surveyId) {
    fetch(`${API_BASE_URL}/odata/Surveys(${surveyId})`, {
        method: 'DELETE',
        headers: getApiHeaders()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            showAlert('Xóa khảo sát thành công', 'success');
            loadSurveys();
        })
        .catch(error => {
            console.error('Error deleting survey:', error);
            showAlert('Không thể xóa khảo sát', 'danger');
        });
}

function changePage(page) {
    currentPage = page;
    loadSurveys();
}

// Event listeners
document.addEventListener('DOMContentLoaded', function () {
    loadSurveys();

    // Search functionality
    const searchInput = document.getElementById('searchInput');
    const searchBtn = document.getElementById('searchBtn');

    searchBtn.addEventListener('click', searchSurveys);

    searchInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            searchSurveys();
        }
    });

    // Delete confirmation
    document.getElementById('confirmDeleteBtn').addEventListener('click', confirmDelete);
});