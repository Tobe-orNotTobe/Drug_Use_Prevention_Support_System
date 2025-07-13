// survey-results.js - Survey results page functionality
$(document).ready(function () {
    // Configuration
    const API_RESULTS_URL = 'https://localhost:7008/api/SurveyResults';
    let CURRENT_USER_ID = window.CURRENT_USER_ID || null;

    let userResults = [];
    let filteredResults = [];

    // Check if user is logged in
    if (!CURRENT_USER_ID || !window.USER_TOKEN) {
        showAlert('warning', 'Bạn cần đăng nhập để xem kết quả khảo sát');
        setTimeout(() => {
            window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }, 2000);
        return;
    }

    // Initialize page
    init();

    function init() {
        loadUserResults();
        bindEvents();
    }

    function bindEvents() {
        // Filter and search functionality
        $('#searchInput').on('input', function () {
            filterAndDisplayResults();
        });

        $('#sortFilter').change(function () {
            filterAndDisplayResults();
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        return {
            'Authorization': `Bearer ${window.USER_TOKEN}`,
            'Content-Type': 'application/json'
        };
    }

    // Load user survey results
    function loadUserResults() {
        showLoading();

        $.ajax({
            url: `${API_RESULTS_URL}/user/${CURRENT_USER_ID}`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                hideLoading();

                if (response.success) {
                    userResults = response.data || [];
                    filteredResults = [...userResults];

                    displayResults();
                    updateStatistics();
                } else {
                    showAlert('error', response.message || 'Đã xảy ra lỗi khi tải kết quả');
                }
            },
            error: function (xhr, status, error) {
                hideLoading();
                console.error('Error loading user results:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    setTimeout(() => {
                        window.location.href = '/Auth/Login';
                    }, 2000);
                } else {
                    showAlert('error', 'Đã xảy ra lỗi khi tải kết quả khảo sát');
                }
            }
        });
    }

    // Filter and display results
    function filterAndDisplayResults() {
        const searchTerm = $('#searchInput').val().toLowerCase().trim();
        const sortFilter = $('#sortFilter').val();

        // Apply search filter
        filteredResults = userResults.filter(result => {
            if (searchTerm && !result.SurveyName.toLowerCase().includes(searchTerm)) {
                return false;
            }
            return true;
        });

        // Apply sorting
        switch (sortFilter) {
            case 'newest':
                filteredResults.sort((a, b) => new Date(b.TakenAt) - new Date(a.TakenAt));
                break;
            case 'oldest':
                filteredResults.sort((a, b) => new Date(a.TakenAt) - new Date(b.TakenAt));
                break;
            case 'highest':
                filteredResults.sort((a, b) => b.TotalScore - a.TotalScore);
                break;
            case 'lowest':
                filteredResults.sort((a, b) => a.TotalScore - b.TotalScore);
                break;
        }

        displayResults();
    }

    // Display results in grid
    function displayResults() {
        const resultsGrid = $('#resultsGrid');
        const emptyState = $('#emptyState');

        resultsGrid.empty();

        if (filteredResults.length === 0) {
            resultsGrid.hide();
            emptyState.show();
            return;
        }

        emptyState.hide();
        resultsGrid.show();

        filteredResults.forEach((result, index) => {
            const scorePercentage = getScorePercentage(result.TotalScore);
            const scoreColor = getScoreColor(scorePercentage);

            const resultCard = `
                <div class="col-lg-4 col-md-6 mb-4" id="result-${result.ResultId}">
                    <div class="card h-100 result-card" data-result-id="${result.ResultId}">
                        <div class="card-header bg-${scoreColor} text-white">
                            <h5 class="card-title mb-0">${escapeHtml(result.SurveyName)}</h5>
                            <span class="badge bg-light text-dark float-end">
                                Điểm: ${result.TotalScore}
                            </span>
                        </div>
                        <div class="card-body">
                            <!-- Score Circle -->
                            <div class="text-center mb-3">
                                <div class="score-circle bg-${scoreColor}">
                                    <span class="score-number">${result.TotalScore}</span>
                                </div>
                            </div>

                            <!-- Date Info -->
                            <div class="text-center mb-3">
                                <small class="text-muted">Ngày thực hiện:</small><br>
                                <strong>${formatDate(result.TakenAt)}</strong>
                            </div>

                            <!-- Recommendation Preview -->
                            ${result.Recommendation ? `
                                <div class="recommendation-preview">
                                    <small class="text-muted">Khuyến nghị:</small>
                                    <p class="text-info small">${escapeHtml(result.Recommendation.substring(0, 80))}${result.Recommendation.length > 80 ? '...' : ''}</p>
                                </div>
                            ` : ''}
                        </div>
                        <div class="card-footer">
                            <div class="btn-group w-100" role="group">
                                <button type="button" 
                                        class="btn btn-info btn-sm" 
                                        onclick="viewResultDetail(${result.ResultId})"
                                        title="Xem chi tiết">
                                    <i class="fas fa-eye"></i> Chi tiết
                                </button>
                                <button type="button" 
                                        class="btn btn-primary btn-sm" 
                                        onclick="retakeSurvey(${result.SurveyId})"
                                        title="Làm lại khảo sát">
                                    <i class="fas fa-redo"></i> Làm lại
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            resultsGrid.append(resultCard);
        });

        // Scroll to specific result if hash is present
        const hash = window.location.hash;
        if (hash && hash.startsWith('#result-')) {
            const targetElement = $(hash);
            if (targetElement.length) {
                $('html, body').animate({
                    scrollTop: targetElement.offset().top - 100
                }, 1000);
                targetElement.addClass('highlight-result');
                setTimeout(() => {
                    targetElement.removeClass('highlight-result');
                }, 3000);
            }
        }
    }

    // Update statistics
    function updateStatistics() {
        const totalResults = userResults.length;
        const averageScore = totalResults > 0 ?
            Math.round(userResults.reduce((sum, result) => sum + result.TotalScore, 0) / totalResults) : 0;
        const highestScore = totalResults > 0 ? Math.max(...userResults.map(r => r.TotalScore)) : 0;
        const latestScore = totalResults > 0 ?
            userResults.sort((a, b) => new Date(b.TakenAt) - new Date(a.TakenAt))[0].TotalScore : 0;

        $('#totalResults').text(totalResults);
        $('#averageScore').text(averageScore);
        $('#highestScore').text(highestScore);
        $('#latestScore').text(latestScore);
    }

    // View result detail
    function viewResultDetail(resultId) {
        const result = userResults.find(r => r.ResultId === resultId);
        if (!result) return;

        const detailContent = `
            <div class="row">
                <div class="col-md-8">
                    <h4>${escapeHtml(result.SurveyName)}</h4>
                    <p class="text-muted">Kết quả khảo sát chi tiết</p>
                </div>
                <div class="col-md-4">
                    <div class="text-center">
                        <div class="score-circle-large bg-${getScoreColor(getScorePercentage(result.TotalScore))}">
                            <span class="score-number-large">${result.TotalScore}</span>
                        </div>
                    </div>
                </div>
            </div>
            
            <hr>
            
            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-calendar-check"></i> Ngày thực hiện:</h6>
                    <p>${formatDateTime(result.TakenAt)}</p>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-star"></i> Tổng điểm:</h6>
                    <p><strong class="text-${getScoreColor(getScorePercentage(result.TotalScore))}">${result.TotalScore}</strong></p>
                </div>
            </div>
            
            ${result.Recommendation ? `
                <div class="mt-4">
                    <h6><i class="fas fa-lightbulb"></i> Khuyến nghị:</h6>
                    <div class="alert alert-info">
                        ${escapeHtml(result.Recommendation)}
                    </div>
                </div>
            ` : ''}
            
            <div class="mt-4">
                <h6><i class="fas fa-chart-line"></i> Phân tích điểm:</h6>
                <div class="progress mb-2" style="height: 25px;">
                    <div class="progress-bar bg-${getScoreColor(getScorePercentage(result.TotalScore))}" 
                         role="progressbar" 
                         style="width: ${getScorePercentage(result.TotalScore)}%">
                        ${getScorePercentage(result.TotalScore)}%
                    </div>
                </div>
                <small class="text-muted">
                    ${getScoreDescription(getScorePercentage(result.TotalScore))}
                </small>
            </div>
        `;

        $('#resultDetailContent').html(detailContent);

        // Setup retake button
        $('#retakeSurveyBtn').show().off('click').on('click', function () {
            retakeSurvey(result.SurveyId);
        });

        new bootstrap.Modal(document.getElementById('resultDetailModal')).show();
    }

    // Retake survey
    function retakeSurvey(surveyId) {
        if (confirm('Bạn có chắc chắn muốn làm lại khảo sát này? Kết quả cũ sẽ được thay thế.')) {
            window.location.href = `/Surveys/Take/${surveyId}`;
        }
    }

    // Utility functions
    function getScorePercentage(score) {
        // Assuming max score is around 30 for most surveys
        // You might want to adjust this based on your survey scoring system
        const maxScore = 30;
        return Math.min(Math.round((score / maxScore) * 100), 100);
    }

    function getScoreColor(percentage) {
        if (percentage >= 70) return 'danger';
        if (percentage >= 40) return 'warning';
        if (percentage >= 20) return 'info';
        return 'success';
    }

    function getScoreDescription(percentage) {
        if (percentage >= 70) return 'Mức độ rủi ro cao - Cần chú ý và can thiệp';
        if (percentage >= 40) return 'Mức độ rủi ro trung bình - Nên theo dõi';
        if (percentage >= 20) return 'Mức độ rủi ro thấp - Tình trạng ổn định';
        return 'Mức độ rủi ro rất thấp - Tình trạng tốt';
    }

    function showLoading() {
        $('#loadingSpinner').show();
        $('#resultsGrid').hide();
        $('#emptyState').hide();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
        $('#resultsGrid').show();
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

    function formatDateTime(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN') + ' ' + date.toLocaleTimeString('vi-VN', {
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // Make functions global for onclick handlers
    window.viewResultDetail = viewResultDetail;
    window.retakeSurvey = retakeSurvey;
});