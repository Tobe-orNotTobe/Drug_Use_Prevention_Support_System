$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata';
    const CURRENT_USER_ID = window.CURRENT_USER_ID || null;
    const USER_PERMISSIONS = window.USER_PERMISSIONS || {};

    let currentPage = 1;
    let pageSize = 10;
    let totalRecords = 0;

    // Initialize page
    init();

    function init() {
        loadSurveys();
        if (USER_PERMISSIONS.isAuthenticated) {
            loadUserSurveyResults();
        }
        if (USER_PERMISSIONS.canViewReports) {
            loadSurveyStatistics();
        }
        bindEvents();
    }

    function bindEvents() {
        // Search and filter functionality
        $('#searchBtn').click(function () {
            currentPage = 1;
            loadSurveys();
        });

        $('#searchInput').keypress(function (e) {
            if (e.which === 13) {
                currentPage = 1;
                loadSurveys();
            }
        });

        $('#typeFilter, #statusFilter').change(function () {
            currentPage = 1;
            loadSurveys();
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        const headers = {
            'Content-Type': 'application/json'
        };

        if (USER_PERMISSIONS.isAuthenticated && window.USER_TOKEN) {
            headers['Authorization'] = `Bearer ${window.USER_TOKEN}`;
        }

        return headers;
    }

    // Load all surveys with filtering
    function loadSurveys() {
        showLoading();

        let odataQuery = `${API_BASE_URL}/Surveys?`;
        let filters = [];

        // Search filter
        const searchTerm = $('#searchInput').val().trim();
        if (searchTerm) {
            filters.push(`contains(tolower(Title), '${searchTerm.toLowerCase()}') or contains(tolower(Description), '${searchTerm.toLowerCase()}')`);
        }

        // Type filter
        const type = $('#typeFilter').val();
        if (type) {
            filters.push(`SurveyType eq '${type}'`);
        }

        // Status filter
        const status = $('#statusFilter').val();
        if (status !== '') {
            filters.push(`IsActive eq ${status}`);
        }

        // Apply filters
        if (filters.length > 0) {
            odataQuery += `$filter=${filters.join(' and ')}&`;
        }

        // Add ordering and pagination
        odataQuery += `$orderby=CreatedAt desc&$top=${pageSize}&$skip=${(currentPage - 1) * pageSize}&$count=true`;

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                hideLoading();
                const surveys = response.value || response;
                displaySurveys(surveys);
                totalRecords = response['@odata.count'] || surveys.length;
                updatePagination();
            },
            error: function (xhr, status, error) {
                hideLoading();
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi tải danh sách khảo sát');
            }
        });
    }

    // Display surveys with role-based action buttons
    function displaySurveys(surveys) {
        const tbody = $('#surveysTableBody');
        tbody.empty();

        if (!surveys || surveys.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="7" class="text-center">Không có khảo sát nào</td>
                </tr>
            `);
            return;
        }

        surveys.forEach(survey => {
            const actionButtons = generateSurveyActionButtons(survey);

            const row = `
                <tr>
                    <td>${survey.SurveyId}</td>
                    <td>
                        <strong>${escapeHtml(survey.Title)}</strong>
                        ${survey.Description ? `<br><small class="text-muted">${escapeHtml(survey.Description.substring(0, 100))}${survey.Description.length > 100 ? '...' : ''}</small>` : ''}
                    </td>
                    <td>
                        <span class="badge bg-info">${getSurveyTypeText(survey.SurveyType)}</span>
                    </td>
                    <td>${survey.QuestionCount || 0}</td>
                    <td>
                        <span class="badge bg-${survey.IsActive ? 'success' : 'secondary'}">
                            ${survey.IsActive ? 'Hoạt động' : 'Không hoạt động'}
                        </span>
                    </td>
                    <td>${formatDate(survey.CreatedAt)}</td>
                    <td>${actionButtons}</td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Generate action buttons based on user permissions
    function generateSurveyActionButtons(survey) {
        let buttons = '';

        // Everyone can view details
        buttons += `<button type="button" class="btn btn-info btn-sm me-1" onclick="viewSurvey(${survey.SurveyId})" title="Xem chi tiết">
                        <i class="fas fa-eye"></i>
                    </button>`;

        // Only authenticated users can take surveys
        if (USER_PERMISSIONS.isAuthenticated && USER_PERMISSIONS.canTakeSurveys && survey.IsActive) {
            buttons += `<button type="button" class="btn btn-success btn-sm me-1" onclick="takeSurvey(${survey.SurveyId})" title="Làm khảo sát">
                            <i class="fas fa-play"></i>
                        </button>`;
        }

        // Only Staff+ can manage surveys
        if (USER_PERMISSIONS.canManageSurveys) {
            buttons += `<button type="button" class="btn btn-warning btn-sm me-1" onclick="editSurvey(${survey.SurveyId})" title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>`;

            buttons += `<button type="button" class="btn btn-danger btn-sm" onclick="deleteSurvey(${survey.SurveyId})" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>`;
        }

        return `<div class="btn-group btn-group-sm" role="group">${buttons}</div>`;
    }

    // Load user's survey results
    function loadUserSurveyResults() {
        if (!CURRENT_USER_ID) return;

        const odataQuery = `${API_BASE_URL}/SurveyResults?$filter=UserId eq ${CURRENT_USER_ID}&$expand=Survey&$orderby=CompletedAt desc`;

        $.ajax({
            url: odataQuery,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const userResults = response.value || response;
                displayUserSurveyResults(userResults);
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi tải kết quả khảo sát');
            }
        });
    }

    // Display user's survey results
    function displayUserSurveyResults(userResults) {
        const tbody = $('#userSurveyResultsTableBody');
        tbody.empty();

        if (!userResults || userResults.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="6" class="text-center">Bạn chưa thực hiện khảo sát nào</td>
                </tr>
            `);
            return;
        }

        userResults.forEach(result => {
            const survey = result.Survey;
            if (!survey) return;

            const riskLevel = getRiskLevel(result.Score, result.MaxScore);
            const recommendation = getRecommendation(riskLevel);

            const row = `
                <tr>
                    <td>
                        <strong>${escapeHtml(survey.Title)}</strong>
                        <br><small class="text-muted">${getSurveyTypeText(survey.SurveyType)}</small>
                    </td>
                    <td>${formatDate(result.CompletedAt)}</td>
                    <td>
                        <strong>${result.Score}/${result.MaxScore}</strong>
                        <br><small class="text-muted">${Math.round((result.Score / result.MaxScore) * 100)}%</small>
                    </td>
                    <td>
                        <span class="badge bg-${getRiskLevelClass(riskLevel)}">
                            ${riskLevel}
                        </span>
                    </td>
                    <td>
                        <small>${recommendation}</small>
                    </td>
                    <td>
                        <button type="button" class="btn btn-info btn-sm" onclick="viewResult(${result.SurveyResultId})" title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });
    }

    // Load survey statistics (for managers)
    function loadSurveyStatistics() {
        $.ajax({
            url: `${API_BASE_URL}/SurveyResults?$count=true`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                const totalCount = response['@odata.count'] || 0;
                $('#totalSurveys').text(totalCount);

                // Load completed surveys
                $.ajax({
                    url: `${API_BASE_URL}/SurveyResults?$filter=CompletedAt ne null&$count=true`,
                    method: 'GET',
                    headers: setupAjaxHeaders(),
                    success: function (response) {
                        const completedCount = response['@odata.count'] || 0;
                        $('#completedSurveys').text(completedCount);
                    }
                });

                // Load high risk surveys (assuming score > 70% is high risk)
                // This would need to be adjusted based on actual scoring logic
                $.ajax({
                    url: `${API_BASE_URL}/SurveyResults?$filter=Score gt 7&$count=true`,
                    method: 'GET',
                    headers: setupAjaxHeaders(),
                    success: function (response) {
                        const highRiskCount = response['@odata.count'] || 0;
                        $('#highRiskSurveys').text(highRiskCount);
                    }
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading survey statistics:', error);
            }
        });
    }

    // Survey action functions
    window.viewSurvey = function (surveyId) {
        window.location.href = `/Surveys/Details/${surveyId}`;
    };

    window.takeSurvey = function (surveyId) {
        if (!USER_PERMISSIONS.isAuthenticated) {
            showAlert('warning', 'Bạn cần đăng nhập để làm khảo sát');
            setTimeout(() => {
                window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            }, 2000);
            return;
        }

        if (!USER_PERMISSIONS.canTakeSurveys) {
            showAlert('error', 'Bạn không có quyền làm khảo sát');
            return;
        }

        window.location.href = `/Surveys/Take/${surveyId}`;
    };

    window.editSurvey = function (surveyId) {
        if (!USER_PERMISSIONS.canManageSurveys) {
            showAlert('error', 'Bạn không có quyền chỉnh sửa khảo sát');
            return;
        }
        window.location.href = `/Surveys/Edit/${surveyId}`;
    };

    window.deleteSurvey = function (surveyId) {
        if (!USER_PERMISSIONS.canManageSurveys) {
            showAlert('error', 'Bạn không có quyền xóa khảo sát');
            return;
        }

        if (confirm('Bạn có chắc chắn muốn xóa khảo sát này?')) {
            deleteSurveyAction(surveyId);
        }
    };

    window.viewResult = function (resultId) {
        window.location.href = `/Surveys/ResultDetail/${resultId}`;
    };

    // Action implementations
    function deleteSurveyAction(surveyId) {
        $.ajax({
            url: `${API_BASE_URL}/Surveys(${surveyId})`,
            method: 'DELETE',
            headers: setupAjaxHeaders(),
            success: function (response) {
                showAlert('success', 'Xóa khảo sát thành công!');
                loadSurveys(); // Reload surveys
                if (USER_PERMISSIONS.canViewReports) {
                    loadSurveyStatistics(); // Reload statistics
                }
            },
            error: function (xhr, status, error) {
                handleAjaxError(xhr, 'Đã xảy ra lỗi khi xóa khảo sát');
            }
        });
    }

    // Helper functions
    function getSurveyTypeText(type) {
        const types = {
            'ASSIST': 'ASSIST',
            'CRAFFT': 'CRAFFT',
            'Custom': 'Tùy chỉnh'
        };
        return types[type] || type;
    }

    function getRiskLevel(score, maxScore) {
        const percentage = (score / maxScore) * 100;
        if (percentage < 30) return 'Thấp';
        if (percentage < 60) return 'Trung bình';
        if (percentage < 80) return 'Cao';
        return 'Rất cao';
    }

    function getRiskLevelClass(riskLevel) {
        const classes = {
            'Thấp': 'success',
            'Trung bình': 'warning',
            'Cao': 'danger',
            'Rất cao': 'dark'
        };
        return classes[riskLevel] || 'secondary';
    }

    function getRecommendation(riskLevel) {
        const recommendations = {
            'Thấp': 'Tiếp tục duy trì lối sống lành mạnh',
            'Trung bình': 'Tham gia các khóa học phòng ngừa',
            'Cao': 'Nên đặt lịch hẹn tư vấn với chuyên gia',
            'Rất cao': 'Cần tư vấn và hỗ trợ chuyên môn ngay lập tức'
        };
        return recommendations[riskLevel] || '';
    }

    function handleAjaxError(xhr, defaultMessage) {
        if (xhr.status === 401) {
            showAlert('error', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
            setTimeout(() => {
                window.location.href = '/Auth/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
            }, 2000);
        } else if (xhr.status === 403) {
            showAlert('error', 'Bạn không có quyền thực hiện hành động này.');
        } else {
            showAlert('error', defaultMessage);
        }
    }

    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function showLoading() {
        $('#loadingSpinner').show();
    }

    function hideLoading() {
        $('#loadingSpinner').hide();
    }

    function showAlert(type, message) {
        const alertClass = type === 'success' ? 'alert-success' : type === 'error' ? 'alert-danger' : 'alert-warning';
        const alertHtml = `<div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                              ${message}
                              <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                           </div>`;
        $('.container-fluid').prepend(alertHtml);

        setTimeout(() => {
            $('.alert').fadeOut();
        }, 5000);
    }

    function updatePagination() {
        const totalPages = Math.ceil(totalRecords / pageSize);
        const pagination = $('#pagination');
        pagination.empty();

        for (let i = 1; i <= totalPages; i++) {
            const activeClass = i === currentPage ? 'active' : '';
            pagination.append(`
                <li class="page-item ${activeClass}">
                    <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
                </li>
            `);
        }
    }

    window.changePage = function (page) {
        currentPage = page;
        loadSurveys();
    };
});