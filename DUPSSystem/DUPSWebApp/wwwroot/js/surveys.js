// surveys.js - Survey list page functionality
$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata';
    const API_RESULTS_URL = 'https://localhost:7008/api/SurveyResults';
    let CURRENT_USER_ID = window.CURRENT_USER_ID || null;

    let allSurveys = [];
    let filteredSurveys = [];
    let userCompletedSurveys = [];

    // Check if user is logged in
    if (!CURRENT_USER_ID || !window.USER_TOKEN) {
        showAlert('warning', 'Bạn cần đăng nhập để xem khảo sát');
        setTimeout(() => {
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }, 2000);
        return;
    }

    // Initialize page
    init();

    function init() {
        loadSurveys();
        loadUserSurveyHistory();
        bindEvents();
    }

    function bindEvents() {
        // Search and filter functionality
        $('#searchBtn').click(function () {
            filterAndDisplaySurveys();
        });

        $('#searchInput').on('input', function () {
            filterAndDisplaySurveys();
        });

        $('#sortFilter').change(function () {
            filterAndDisplaySurveys();
        });

        // Modal events
        $('#takeSurveyBtn').click(function () {
            const surveyId = $(this).data('survey-id');
            window.location.href = `/Surveys/Take/${surveyId}`;
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        return {
            'Authorization': `Bearer ${window.USER_TOKEN}`,
            'Content-Type': 'application/json'
        };
    }

    // Load all surveys
    function loadSurveys() {
        showLoading();

        const odataQuery = `${API_BASE_URL}/Surveys?$orderby=CreatedAt desc`;

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                allSurveys = response.value || response || [];
                filteredSurveys = [...allSurveys];

                displaySurveys();
                updateStatistics();
                hideLoading();
            },
            error: function (xhr, status, error) {
                hideLoading();
                console.error('Error loading surveys:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 2000);
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi tải danh sách khảo sát');
                }
            }
        });
    }

    // Load user survey history for statistics
    function loadUserSurveyHistory() {
        $.ajax({
            url: `${API_RESULTS_URL}/user/${CURRENT_USER_ID}`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                if (response.success) {
                    userCompletedSurveys = response.data || [];
                    updateStatistics();
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading user survey history:', error);
            }
        });
    }

    // Filter and display surveys
    function filterAndDisplaySurveys() {
        const searchTerm = $('#searchInput').val().toLowerCase().trim();
        const sortFilter = $('#sortFilter').val();

        // Apply search filter
        filteredSurveys = allSurveys.filter(survey => {
            if (searchTerm && !survey.Name.toLowerCase().includes(searchTerm) &&
                (!survey.Description || !survey.Description.toLowerCase().includes(searchTerm))) {
                return false;
            }
            return true;
        });

        // Apply sorting
        switch (sortFilter) {
            case 'newest':
                filteredSurveys.sort((a, b) => new Date(b.CreatedAt) - new Date(a.CreatedAt));
                break;
            case 'oldest':
                filteredSurveys.sort((a, b) => new Date(a.CreatedAt) - new Date(b.CreatedAt));
                break;
            case 'name':
                filteredSurveys.sort((a, b) => a.Name.localeCompare(b.Name));
                break;
        }

        displaySurveys();
    }

    // Display surveys in grid
    function displaySurveys() {
        const surveysGrid = $('#surveysGrid');
        const emptyState = $('#emptyState');

        surveysGrid.empty();

        if (filteredSurveys.length === 0) {
            surveysGrid.hide();
            emptyState.show();
            return;
        }

        emptyState.hide();
        surveysGrid.show();

        filteredSurveys.forEach(survey => {
            const isCompleted = userCompletedSurveys.some(us => us.SurveyId === survey.SurveyId);
            const completedResult = userCompletedSurveys.find(us => us.SurveyId === survey.SurveyId);

            const surveyCard = `
                <div class="col-lg-4 col-md-6 mb-4">
                    <div class="card h-100 survey-card" data-survey-id="${survey.SurveyId}">
                        <div class="card-header ${isCompleted ? 'bg-success' : 'bg-primary'} text-white">
                            <h5 class="card-title mb-0">${escapeHtml(survey.Name)}</h5>
                            ${isCompleted ? `
                                <span class="badge bg-light text-dark float-end">
                                    <i class="fas fa-check"></i> Đã hoàn thành
                                </span>
                            ` : `
                                <span class="badge bg-light text-dark float-end">
                                    <i class="fas fa-play"></i> Chưa làm
                                </span>
                            `}
                        </div>
                        <div class="card-body">
                            <p class="card-text">${survey.Description ? escapeHtml(survey.Description.substring(0, 120)) + (survey.Description.length > 120 ? '...' : '') : 'Không có mô tả'}</p>
                            
                            ${isCompleted ? `
                                <div class="mb-3">
                                    <div class="row text-center">
                                        <div class="col-6">
                                            <small class="text-muted">Điểm số:</small><br>
                                            <strong class="text-success">${completedResult.TotalScore}</strong>
                                        </div>
                                        <div class="col-6">
                                            <small class="text-muted">Ngày làm:</small><br>
                                            <strong>${formatDate(completedResult.TakenAt)}</strong>
                                        </div>
                                    </div>
                                </div>
                            ` : ''}

                            <div class="text-center">
                                <small class="text-muted">Ngày tạo: ${formatDate(survey.CreatedAt)}</small>
                            </div>
                        </div>
                        <div class="card-footer">
                            <div class="btn-group w-100" role="group">
                                <button type="button" 
                                        class="btn btn-info btn-sm" 
                                        onclick="viewSurveyDetail(${survey.SurveyId})"
                                        title="Xem chi tiết">
                                    <i class="fas fa-eye"></i> Chi tiết
                                </button>
                                ${isCompleted ? `
                                    <button type="button" 
                                            class="btn btn-success btn-sm" 
                                            onclick="viewResult(${completedResult.ResultId})"
                                            title="Xem kết quả">
                                        <i class="fas fa-chart-bar"></i> Kết quả
                                    </button>
                                ` : `
                                    <button type="button" 
                                            class="btn btn-primary btn-sm" 
                                            onclick="startSurvey(${survey.SurveyId})"
                                            title="Bắt đầu khảo sát">
                                        <i class="fas fa-play"></i> Bắt đầu
                                    </button>
                                `}
                            </div>
                        </div>
                    </div>
                </div>
            `;

            surveysGrid.append(surveyCard);
        });
    }

    // Update statistics
    function updateStatistics() {
        const totalSurveys = allSurveys.length;
        const completedSurveys = userCompletedSurveys.length;
        const availableSurveys = totalSurveys - completedSurveys;
        const avgScore = completedSurveys > 0 ?
            Math.round(userCompletedSurveys.reduce((sum, us) => sum + us.TotalScore, 0) / completedSurveys) : 0;

        $('#totalSurveys').text(totalSurveys);
        $('#completedSurveys').text(completedSurveys);
        $('#availableSurveys').text(availableSurveys);
        $('#avgScore').text(avgScore);
    }

    // View survey detail
    function viewSurveyDetail(surveyId) {
        const survey = allSurveys.find(s => s.SurveyId === surveyId);
        if (!survey) return;

        const isCompleted = userCompletedSurveys.some(us => us.SurveyId === surveyId);
        const completedResult = userCompletedSurveys.find(us => us.SurveyId === surveyId);

        const detailContent = `
            <div class="row">
                <div class="col-md-8">
                    <h4>${escapeHtml(survey.Name)}</h4>
                    <p class="text-muted">${survey.Description ? escapeHtml(survey.Description) : 'Không có mô tả'}</p>
                </div>
                <div class="col-md-4">
                    <span class="badge ${isCompleted ? 'bg-success' : 'bg-primary'} fs-6 p-2">
                        ${isCompleted ? 'Đã hoàn thành' : 'Chưa thực hiện'}
                    </span>
                </div>
            </div>
            
            <hr>
            
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-calendar-plus"></i> Ngày tạo:</h6>
                    <p>${formatDate(survey.CreatedAt)}</p>
                </div>
                ${isCompleted ? `
                    <div class="col-md-6">
                        <h6><i class="fas fa-calendar-check"></i> Ngày hoàn thành:</h6>
                        <p>${formatDate(completedResult.TakenAt)}</p>
                    </div>
                ` : ''}
            </div>
            
            ${isCompleted ? `
                <div class="row">
                    <div class="col-md-6">
                        <h6><i class="fas fa-star"></i> Điểm số:</h6>
                        <p><strong class="text-success">${completedResult.TotalScore}</strong></p>
                    </div>
                    <div class="col-md-6">
                        <h6><i class="fas fa-lightbulb"></i> Khuyến nghị:</h6>
                        <p class="text-info">${completedResult.Recommendation || 'Không có khuyến nghị'}</p>
                    </div>
                </div>
                <div class="alert alert-success">
                    <i class="fas fa-check-circle"></i>
                    Bạn đã hoàn thành khảo sát này. Có thể xem chi tiết kết quả trong mục "Kết quả của tôi".
                </div>
            ` : `
                <div class="alert alert-info">
                    <i class="fas fa-info-circle"></i>
                    Bạn chưa thực hiện khảo sát này. Hãy bắt đầu để nhận được đánh giá và khuyến nghị phù hợp.
                </div>
            `}
        `;

        $('#surveyDetailContent').html(detailContent);

        // Setup modal buttons
        if (isCompleted) {
            $('#takeSurveyBtn').hide();
        } else {
            $('#takeSurveyBtn').show().data('survey-id', surveyId);
        }

        new bootstrap.Modal(document.getElementById('surveyDetailModal')).show();
    }

    // Start survey
    function startSurvey(surveyId) {
        window.location.href = `/Surveys/Take/${surveyId}`;
    }

    // View result
    function viewResult(resultId) {
        window.location.href = `/Surveys/Results#result-${resultId}`;
    }

    // Utility functions
    function showLoading() {
        $('#loadingSpinner').show();
        $('#surveysGrid').hide();
        $('#emptyState').hide();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
        $('#surveysGrid').show();
    }

    function showAlert(type, message) {
        const alertClass = {
            'success': 'alert-success',
            'error': 'alert-danger',
            'warning': 'alert-warning',
            'info': 'alert-info'
        }[type] || 'alert-info';

        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show position-fixed" 
                 style="top: 20px; right: 20px; z-index: 1050; min-width: 300px;" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        // Remove existing alerts
        $('.alert.position-fixed').remove();

        // Add new alert
        $('body').append(alertHtml);

        // Auto remove after 5 seconds
        setTimeout(() => {
            $('.alert.position-fixed').fadeOut();
        }, 5000);
    }

    function escapeHtml(text) {
        if (!text) return '';
        return text
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    function formatDate(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    // Make functions global for onclick handlers
    window.viewSurveyDetail = viewSurveyDetail;
    window.startSurvey = startSurvey;
    window.viewResult = viewResult;
});