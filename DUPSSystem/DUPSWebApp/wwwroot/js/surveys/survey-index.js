const API_BASE_URL = document.querySelector('meta[name="api-base-url"]')?.getAttribute('content') || 'https://localhost:7008';

let currentPage = 1;
let pageSize = 10;
let searchTerm = '';
let deleteId = null;
let totalCount = 0;

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

    let url = `${API_BASE_URL}/odata/Surveys?$skip=${(currentPage - 1) * pageSize}&$top=${pageSize}&$count=true&$orderby=CreatedAt desc`;

    if (searchTerm) {
        url += `&$filter=contains(tolower(Name), '${searchTerm.toLowerCase()}')`;
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
            totalCount = data['@odata.count'] || 0;
            displaySurveys(data.value || data);
            updatePagination();
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
                <td colspan="5" class="text-center text-muted py-4">
                    <i class="fas fa-inbox fa-3x mb-3 text-muted"></i>
                    <h5 class="text-muted">Không có khảo sát nào</h5>
                    <p class="text-muted">Chưa có khảo sát nào được tạo hoặc không tìm thấy kết quả phù hợp</p>
                </td>
            </tr>
        `;
        return;
    }

    surveys.forEach(survey => {
        const createdAt = survey.CreatedAt ? new Date(survey.CreatedAt).toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit'
        }) : 'Không có';

        const row = `
            <tr>
                <td>
                    <div class="d-flex align-items-start">
                        <div>
                            <strong class="d-block">${survey.Name}</strong>
                            ${survey.Description ? `<small class="text-muted">${survey.Description.length > 100 ? survey.Description.substring(0, 100) + '...' : survey.Description}</small>` : ''}
                        </div>
                    </div>
                </td>
                <td>
                    <small class="text-muted">${createdAt}</small>
                </td>
                <td>
                    <div class="btn-group" role="group">
                        <a href="/Surveys/Details/${survey.SurveyId}" class="btn btn-sm btn-outline-info" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </a>
                        <a href="/Surveys/Edit/${survey.SurveyId}" class="btn btn-sm btn-outline-warning" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </a>
                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="showDeleteModal(${survey.SurveyId}, '${survey.Name.replace(/'/g, "\\'")}')" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
        tbody.insertAdjacentHTML('beforeend', row);
    });
}

function updatePagination() {
    const totalPages = Math.ceil(totalCount / pageSize);
    const pagination = document.getElementById('pagination');

    if (!pagination || totalPages <= 1) {
        if (pagination) pagination.innerHTML = '';
        return;
    }

    let paginationHtml = '';

    if (currentPage > 1) {
        paginationHtml += `
            <li class="page-item">
                <button class="page-link" onclick="changePage(${currentPage - 1})">
                    <i class="fas fa-chevron-left"></i> Trước
                </button>
            </li>`;
    }

    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    for (let i = startPage; i <= endPage; i++) {
        const activeClass = i === currentPage ? 'active' : '';
        paginationHtml += `
            <li class="page-item ${activeClass}">
                <button class="page-link" onclick="changePage(${i})">${i}</button>
            </li>`;
    }

    if (currentPage < totalPages) {
        paginationHtml += `
            <li class="page-item">
                <button class="page-link" onclick="changePage(${currentPage + 1})">
                    Sau <i class="fas fa-chevron-right"></i>
                </button>
            </li>`;
    }

    pagination.innerHTML = paginationHtml;

    const pageInfo = document.getElementById('pageInfo');
    if (pageInfo) {
        const startItem = (currentPage - 1) * pageSize + 1;
        const endItem = Math.min(currentPage * pageSize, totalCount);
        pageInfo.textContent = `Hiển thị ${startItem}-${endItem} trong tổng số ${totalCount} khảo sát`;
    }
}

function searchSurveys() {
    searchTerm = document.getElementById('searchInput').value.trim();
    currentPage = 1;
    loadSurveys();
}

function clearSearch() {
    document.getElementById('searchInput').value = '';
    searchTerm = '';
    currentPage = 1;
    loadSurveys();
}

function showDeleteModal(id, name) {
    deleteId = id;
    const modal = document.getElementById('deleteModal');
    const modalBody = modal.querySelector('.modal-body');

    modalBody.innerHTML = `
        ${warningText}
        <p>Bạn có chắc chắn muốn xóa khảo sát <strong>"${name}"</strong> không?</p>
        <p class="text-muted small">Hành động này không thể hoàn tác. Tất cả dữ liệu liên quan đến khảo sát này sẽ bị xóa vĩnh viễn.</p>
    `;

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
    const confirmBtn = document.getElementById('confirmDeleteBtn');
    const originalText = confirmBtn.innerHTML;

    confirmBtn.disabled = true;
    confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';

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
            showAlert('Không thể xóa khảo sát. Có thể khảo sát này đang được sử dụng.', 'danger');
        })
        .finally(() => {
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = originalText;
        });
}

function changePage(page) {
    currentPage = page;
    loadSurveys();
}

function refreshSurveys() {
    currentPage = 1;
    loadSurveys();
}

document.addEventListener('DOMContentLoaded', function () {
    loadSurveys();

    const searchInput = document.getElementById('searchInput');
    const searchBtn = document.getElementById('searchBtn');
    const clearBtn = document.getElementById('clearSearchBtn');

    if (searchBtn) searchBtn.addEventListener('click', searchSurveys);
    if (clearBtn) clearBtn.addEventListener('click', clearSearch);

    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                searchSurveys();
            }
        });
    }

    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', confirmDelete);
    }

    const refreshBtn = document.getElementById('refreshBtn');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', refreshSurveys);
    }
});