$(document).ready(function () {
    // Configuration
    const API_BASE_URL = 'https://localhost:7008/odata';
    const API_RESULTS_URL = 'https://localhost:7008/api/SurveyResults';
    let CURRENT_USER_ID = window.CURRENT_USER_ID || null;

    let surveyData = null;
    let currentQuestionIndex = 0;
    let answers = [];

    // Check if user is logged in
    if (!CURRENT_USER_ID || !window.USER_TOKEN) {
        showError('Bạn cần đăng nhập để thực hiện khảo sát');
        return;
    }

    // Check if survey ID is provided
    if (!SURVEY_ID) {
        showError('Không tìm thấy khảo sát');
        return;
    }

    // Initialize page
    init();

    function init() {
        checkIfUserTakenSurvey();
        bindEvents();
    }

    function bindEvents() {
        // Navigation buttons
        $('#prevBtn').click(function () {
            if (currentQuestionIndex > 0) {
                saveCurrentAnswer();
                currentQuestionIndex--;
                displayQuestion();
                updateProgress();
                updateNavigationButtons();
            }
        });

        $('#nextBtn').click(function () {
            if (validateCurrentAnswer()) {
                saveCurrentAnswer();
                currentQuestionIndex++;
                displayQuestion();
                updateProgress();
                updateNavigationButtons();
            }
        });

        $('#submitBtn').click(function () {
            if (validateCurrentAnswer()) {
                saveCurrentAnswer();
                $('#confirmSubmitModal').modal('show');
            }
        });

        $('#confirmSubmitBtn').click(function () {
            submitSurvey();
        });
    }

    // Setup AJAX headers with authentication
    function setupAjaxHeaders() {
        return {
            'Authorization': `Bearer ${window.USER_TOKEN}`,
            'Content-Type': 'application/json'
        };
    }

    // Check if user has already taken this survey
    function checkIfUserTakenSurvey() {
        $.ajax({
            url: `${API_RESULTS_URL}/check/${CURRENT_USER_ID}/${SURVEY_ID}`,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (response) {
                if (response.success && response.data.hasTaken) {
                    showAlreadyTaken();
                } else {
                    loadSurveyData();
                }
            },
            error: function (xhr, status, error) {
                console.error('Error checking survey status:', error);
                loadSurveyData(); // Continue loading anyway
            }
        });
    }

    // Load survey data with questions and options
    function loadSurveyData() {
        // TRY MULTIPLE ENDPOINTS
        const endpoints = [
            `https://localhost:7008/api/Surveys/${SURVEY_ID}/Details`,
            `${API_BASE_URL}/Surveys(${SURVEY_ID})/Details`
        ];

        function tryEndpoint(index) {
            if (index >= endpoints.length) {
                showError('Không thể tải khảo sát từ tất cả endpoints');
                return;
            }

            $.ajax({
                url: endpoints[index],
                method: 'GET',
                headers: setupAjaxHeaders(),
                success: function (response) {
                    console.log('Survey data loaded:', response);
                    if (response.success && response.data) {
                        surveyData = response.data;
                        loadSurveyQuestions();
                    } else {
                        showError(response.message || 'Không thể tải khảo sát');
                    }
                },
                error: function (xhr, status, error) {
                    console.error(`Error loading from endpoint ${index}:`, error);
                    console.error('Response:', xhr.responseText);

                    if (xhr.status === 401) {
                        showError('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    } else if (index < endpoints.length - 1) {
                        // Try next endpoint
                        tryEndpoint(index + 1);
                    } else {
                        // Last endpoint failed
                        if (xhr.status === 404) {
                            showError('Không tìm thấy khảo sát');
                        } else {
                            showError('Đã xảy ra lỗi khi tải khảo sát: ' + (xhr.responseJSON?.message || error));
                        }
                    }
                }
            });
        }

        tryEndpoint(0);
    }
    function loadSurveyQuestions() {
        const url = `${API_BASE_URL}/SurveyQuestions?$filter=SurveyId eq ${SURVEY_ID}&$expand=Options`;

        $.ajax({
            url: url,
            method: 'GET',
            headers: setupAjaxHeaders(),
            success: function (odataResponse) {
                const questions = odataResponse.value || odataResponse;

                if (!questions || questions.length === 0) {
                    showError('Khảo sát không có câu hỏi');
                    return;
                }

                // Gán list Questions kèm Options vào surveyData
                surveyData.Questions = questions;
                initializeSurvey();
            },
            error: function (xhr, status, error) {
                console.error('Error loading survey questions:', error);
                showError('Đã xảy ra lỗi khi tải câu hỏi khảo sát');
            }
        });
    }

    // Initialize survey display
    function initializeSurvey() {
        if (!surveyData || !surveyData.Questions || surveyData.Questions.length === 0) {
            showError('Khảo sát không có câu hỏi');
            return;
        }

        // Initialize answers array
        answers = new Array(surveyData.Questions.length).fill(null);

        // Update survey info
        $('#surveyTitle').html(`<i class="fas fa-clipboard-list me-2"></i>${escapeHtml(surveyData.Name)}`);
        $('#surveyName').text(surveyData.Name);
        $('#surveyDescription').text(surveyData.Description || '');
        $('#totalQuestions').text(surveyData.Questions.length);

        // Show survey form
        $('#loadingSpinner').hide();
        $('#surveyForm').show();

        // Display first question
        currentQuestionIndex = 0;
        displayQuestion();
        updateProgress();
        updateNavigationButtons();
    }

    // Display current question
    function displayQuestion() {
        const question = surveyData.Questions[currentQuestionIndex];
        const container = $('#questionsContainer');

        let questionHtml = `
            <div class="question-container mb-4" data-question-id="${question.QuestionId}">
                <h5 class="mb-3">
                    <span class="badge bg-primary me-2">${currentQuestionIndex + 1}</span>
                    ${escapeHtml(question.QuestionText)}
                </h5>
                <div class="options-container">
        `;

        // Generate options based on question type
        switch (question.QuestionType.toLowerCase()) {
            case 'singlechoice':
                question.Options.forEach((option, index) => {
                    const isChecked = answers[currentQuestionIndex] && answers[currentQuestionIndex].optionId === option.OptionId ? 'checked' : '';
                    questionHtml += `
                        <div class="form-check mb-2">
                            <input class="form-check-input" type="radio" 
                                   name="question_${question.QuestionId}" 
                                   id="option_${option.OptionId}" 
                                   value="${option.OptionId}" 
                                   data-score="${option.Score || 0}"
                                   ${isChecked}>
                            <label class="form-check-label" for="option_${option.OptionId}">
                                ${escapeHtml(option.OptionText)}
                            </label>
                        </div>
                    `;
                });
                break;

            case 'multiplechoice':
                question.Options.forEach((option, index) => {
                    const isChecked = answers[currentQuestionIndex] &&
                        answers[currentQuestionIndex].optionIds &&
                        answers[currentQuestionIndex].optionIds.includes(option.OptionId) ? 'checked' : '';
                    questionHtml += `
                        <div class="form-check mb-2">
                            <input class="form-check-input" type="checkbox" 
                                   name="question_${question.QuestionId}" 
                                   id="option_${option.OptionId}" 
                                   value="${option.OptionId}" 
                                   data-score="${option.Score || 0}"
                                   ${isChecked}>
                            <label class="form-check-label" for="option_${option.OptionId}">
                                ${escapeHtml(option.OptionText)}
                            </label>
                        </div>
                    `;
                });
                break;

            case 'textinput':
                const textValue = answers[currentQuestionIndex] ? answers[currentQuestionIndex].answerText || '' : '';
                questionHtml += `
                    <div class="mb-3">
                        <textarea class="form-control" 
                                  name="question_${question.QuestionId}" 
                                  id="text_${question.QuestionId}" 
                                  rows="4" 
                                  placeholder="Nhập câu trả lời của bạn...">${textValue}</textarea>
                    </div>
                `;
                break;
        }

        questionHtml += `
                </div>
            </div>
        `;

        container.html(questionHtml);
    }

    // Validate current answer
    function validateCurrentAnswer() {
        const question = surveyData.Questions[currentQuestionIndex];
        const questionType = question.QuestionType.toLowerCase();

        switch (questionType) {
            case 'singlechoice':
                const radioChecked = $(`input[name="question_${question.QuestionId}"]:checked`).length > 0;
                if (!radioChecked) {
                    showAlert('warning', 'Vui lòng chọn một đáp án');
                    return false;
                }
                break;

            case 'multiplechoice':
                const checkboxChecked = $(`input[name="question_${question.QuestionId}"]:checked`).length > 0;
                if (!checkboxChecked) {
                    showAlert('warning', 'Vui lòng chọn ít nhất một đáp án');
                    return false;
                }
                break;

            case 'textinput':
                const textValue = $(`#text_${question.QuestionId}`).val().trim();
                if (!textValue) {
                    showAlert('warning', 'Vui lòng nhập câu trả lời');
                    return false;
                }
                break;
        }

        return true;
    }

    // Save current answer
    function saveCurrentAnswer() {
        const question = surveyData.Questions[currentQuestionIndex];
        const questionType = question.QuestionType.toLowerCase();

        let answer = {
            questionId: question.QuestionId,
            score: 0
        };

        switch (questionType) {
            case 'singlechoice':
                const selectedOption = $(`input[name="question_${question.QuestionId}"]:checked`);
                if (selectedOption.length > 0) {
                    answer.optionId = parseInt(selectedOption.val());
                    answer.score = parseInt(selectedOption.data('score')) || 0;
                }
                break;

            case 'multiplechoice':
                const selectedOptions = $(`input[name="question_${question.QuestionId}"]:checked`);
                if (selectedOptions.length > 0) {
                    answer.optionIds = [];
                    answer.score = 0;
                    selectedOptions.each(function () {
                        answer.optionIds.push(parseInt($(this).val()));
                        answer.score += parseInt($(this).data('score')) || 0;
                    });
                }
                break;

            case 'textinput':
                const textValue = $(`#text_${question.QuestionId}`).val().trim();
                if (textValue) {
                    answer.answerText = textValue;
                    answer.score = 1; // Default score for text input
                }
                break;
        }

        answers[currentQuestionIndex] = answer;
    }

    // Update progress bar
    function updateProgress() {
        const progress = ((currentQuestionIndex + 1) / surveyData.Questions.length) * 100;
        $('#progressBar').css('width', `${progress}%`);
        $('#currentQuestion').text(currentQuestionIndex + 1);
    }

    // Update navigation buttons
    function updateNavigationButtons() {
        const isFirst = currentQuestionIndex === 0;
        const isLast = currentQuestionIndex === surveyData.Questions.length - 1;

        $('#prevBtn').prop('disabled', isFirst);

        if (isLast) {
            $('#nextBtn').hide();
            $('#submitBtn').show();
        } else {
            $('#nextBtn').show();
            $('#submitBtn').hide();
        }
    }

    // Submit survey
    function submitSurvey() {
        const submitData = {
            userId: CURRENT_USER_ID,
            surveyId: SURVEY_ID,
            answers: []
        };

        // Prepare answers for submission
        answers.forEach((answer, index) => {
            if (answer) {
                const question = surveyData.Questions[index];
                const questionType = question.QuestionType.toLowerCase();

                if (questionType === 'multiplechoice' && answer.optionIds) {
                    // For multiple choice, create separate entries for each selected option
                    answer.optionIds.forEach(optionId => {
                        const option = question.Options.find(o => o.OptionId === optionId);
                        submitData.answers.push({
                            questionId: answer.questionId,
                            optionId: optionId,
                            score: option ? (option.Score || 0) : 0
                        });
                    });
                } else {
                    submitData.answers.push({
                        questionId: answer.questionId,
                        optionId: answer.optionId || null,
                        answerText: answer.answerText || null,
                        score: answer.score
                    });
                }
            }
        });

        // Show loading state
        $('#confirmSubmitBtn').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang gửi...');

        $.ajax({
            url: `${API_RESULTS_URL}/submit`,
            method: 'POST',
            headers: setupAjaxHeaders(),
            data: JSON.stringify(submitData),
            success: function (response) {
                $('#confirmSubmitModal').modal('hide');

                if (response.Success) {
                    showResult(response);
                } else {
                    showAlert('error', response.Message || 'Đã xảy ra lỗi khi gửi khảo sát');
                }
            },
            error: function (xhr, status, error) {
                $('#confirmSubmitModal').modal('hide');
                console.error('Error submitting survey:', error);

                if (xhr.status === 401) {
                    showAlert('error', 'Phiên đăng nhập đã hết hạn');
                } else {
                    const errorMessage = xhr.responseJSON?.Message || 'Đã xảy ra lỗi khi gửi khảo sát';
                    showAlert('error', errorMessage);
                }
            },
            complete: function () {
                $('#confirmSubmitBtn').prop('disabled', false).html('<i class="fas fa-check"></i> Xác nhận gửi');
            }
        });
    }

    // Show survey result
    function showResult(result) {
        const resultContent = `
            <div class="text-center mb-4">
                <i class="fas fa-trophy fa-3x text-warning mb-3"></i>
                <h4>Chúc mừng bạn đã hoàn thành khảo sát!</h4>
            </div>
            
            <div class="row">
                <div class="col-md-6">
                    <div class="card bg-light">
                        <div class="card-body text-center">
                            <h5 class="card-title">Tổng điểm</h5>
                            <h2 class="text-primary">${result.TotalScore}</h2>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card bg-light">
                        <div class="card-body text-center">
                            <h5 class="card-title">Số câu hỏi</h5>
                            <h2 class="text-info">${surveyData.Questions.length}</h2>
                        </div>
                    </div>
                </div>
            </div>
            
            ${result.Recommendation ? `
                <div class="alert alert-info mt-4">
                    <h6><i class="fas fa-lightbulb"></i> Khuyến nghị:</h6>
                    <p class="mb-0">${escapeHtml(result.Recommendation)}</p>
                </div>
            ` : ''}
            
            <div class="alert alert-success mt-4">
                <i class="fas fa-check-circle"></i>
                Kết quả của bạn đã được lưu thành công. Bạn có thể xem lại trong mục "Kết quả của tôi".
            </div>
        `;

        $('#resultContent').html(resultContent);
        $('#resultModal').modal('show');
    }

    // Show error state
    function showError(message) {
        $('#loadingSpinner').hide();
        $('#surveyForm').hide();
        $('#errorMessage').text(message);
        $('#errorState').show();
    }

    // Show already taken state
    function showAlreadyTaken() {
        $('#loadingSpinner').hide();
        $('#surveyForm').hide();
        $('#alreadyTakenState').show();
    }

    // Utility functions
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
});