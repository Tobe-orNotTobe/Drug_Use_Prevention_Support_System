let appointmentStatusChart, userRoleChart, appointmentTrendsChart, courseAudienceChart, monthlyRegistrationsChart;

const API_BASE_URL = 'https://localhost:7008/api';

const CHART_COLORS = {
    primary: '#4e73df',
    success: '#1cc88a',
    info: '#36b9cc',
    warning: '#f6c23e',
    danger: '#e74a3b',
    secondary: '#858796'
};

const PIE_COLORS = ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796', '#343a40'];

$(document).ready(function () {
    loadDashboard();
});

async function loadDashboard() {
    showLoading(true);

    try {
        await loadDashboardStats();
        await Promise.all([
            loadAppointmentTrends(),
            loadTopConsultants(),
            loadTopCourses(),
            loadUpcomingAppointments(),
            loadPopularSurveys(),
            loadMonthlyRegistrations()
        ]);
    } catch (error) {
        showAlert('Error loading dashboard data.', 'danger');
    } finally {
        showLoading(false);
    }
}

async function loadDashboardStats() {
    try {
        const response = await makeApiCall('/Dashboard/stats');

        if (response && response.success && response.data) {
            updateSummaryCards(response.data);
            updateAppointmentStatusChart(response.data.appointments.statusCounts);
            updateUserRoleChart(response.data.users.roleCounts);
            updateCourseAudienceChart(response.data.courses.coursesByAudience);
            updateSurveyStats(response.data.surveys);
        } else {
            showAlert('Không có dữ liệu Dashboard từ API.', 'warning');
        }
    } catch (error) {
        showAlert('Lỗi khi tải Dashboard từ API.', 'danger');
        throw error;
    }
}

function updateSummaryCards(data) {
    $('#totalUsers').text(data.users.totalUsers);
    $('#activeUsers').text(data.users.activeUsers);
    $('#totalConsultants').text(data.consultants.totalConsultants);
    $('#activeConsultants').text(data.consultants.activeConsultants);
    $('#totalAppointments').text(data.appointments.totalAppointments);
    $('#totalCourses').text(data.courses.totalCourses);
}

function updateSurveyStats(surveyData) {
    $('#totalSurveys').text(surveyData.totalSurveys);
    $('#totalQuestions').text(surveyData.totalQuestions);
    $('#totalResponses').text(surveyData.totalResponses);

    const avgResponses = surveyData.totalSurveys > 0 ?
        Math.round(surveyData.totalResponses / surveyData.totalSurveys) : 0;
    $('#avgResponsesPerSurvey').text(avgResponses);
}

async function loadAppointmentTrends() {
    try {
        const response = await makeApiCall('/Dashboard/appointments/trends?days=30');
        if (response.success && response.data) {
            updateAppointmentTrendsChart(response.data);
        }
    } catch (error) {
        showAlert('Lỗi khi tải dữ liệu xu hướng lịch hẹn.', 'danger');
    }
}

async function loadMonthlyRegistrations() {
    try {
        const response = await makeApiCall('/Dashboard/users/registrations?months=12');
        if (response && Array.isArray(response)) {
            updateMonthlyRegistrationsChart(response);
        }
    } catch (error) {
        showAlert('Lỗi khi tải dữ liệu đăng ký hàng tháng.', 'danger');
    }
}

async function loadTopConsultants() {
    try {
        const response = await makeApiCall('/Dashboard/consultants/top?topN=5');
        if (response && Array.isArray(response)) {
            updateTopConsultantsTable(response);
        }
    } catch (error) {
        showAlert('Lỗi khi tải dữ liệu tư vấn viên.', 'danger');
    }
}

async function loadTopCourses() {
    try {
        const response = await makeApiCall('/Dashboard/courses/top?topN=5');
        if (response && Array.isArray(response)) {
            updateTopCoursesTable(response);
        }
    } catch (error) {
        showAlert('Lỗi khi tải dữ liệu khóa học.', 'danger');
    }
}

async function loadUpcomingAppointments() {
    try {
        const response = await makeApiCall('/Dashboard/appointments/upcoming?days=7');
        if (response && Array.isArray(response)) {
            updateUpcomingAppointmentsTable(response);
        }
    } catch (error) {
        showAlert('Lỗi khi tải dữ liệu lịch hẹn sắp tới.', 'danger');
    }
}

async function loadPopularSurveys() {
    try {
        const response = await makeApiCall('/Dashboard/surveys/popular?topN=5');
        if (response && Array.isArray(response)) {
            updatePopularSurveysTable(response);
        }
    } catch (error) {
        showAlert('Lỗi khi tải dữ liệu khảo sát.', 'danger');
    }
}

function updateAppointmentStatusChart(statusCounts) {
    const ctx = document.getElementById('appointmentStatusChart').getContext('2d');

    if (appointmentStatusChart) {
        appointmentStatusChart.destroy();
    }

    appointmentStatusChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Pending', 'Confirmed', 'Completed', 'Cancelled'],
            datasets: [{
                data: [
                    statusCounts.pending,
                    statusCounts.confirmed,
                    statusCounts.completed,
                    statusCounts.cancelled
                ],
                backgroundColor: [CHART_COLORS.warning, CHART_COLORS.info, CHART_COLORS.success, CHART_COLORS.danger],
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function updateUserRoleChart(roleCounts) {
    const ctx = document.getElementById('userRoleChart').getContext('2d');

    if (userRoleChart) {
        userRoleChart.destroy();
    }

    userRoleChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ['Admin', 'Member', 'Consultant', 'Manager', 'Staff'],
            datasets: [{
                data: [
                    roleCounts.admin,
                    roleCounts.member,
                    roleCounts.consultant,
                    roleCounts.manager,
                    roleCounts.staff
                ],
                backgroundColor: PIE_COLORS,
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function updateAppointmentTrendsChart(trendsData) {
    const ctx = document.getElementById('appointmentTrendsChart').getContext('2d');

    if (appointmentTrendsChart) {
        appointmentTrendsChart.destroy();
    }

    appointmentTrendsChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: trendsData.map(item => item.formattedDate),
            datasets: [{
                label: 'Daily Appointments',
                data: trendsData.map(item => item.count),
                borderColor: CHART_COLORS.primary,
                backgroundColor: CHART_COLORS.primary + '20',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
}

function updateCourseAudienceChart(audienceData) {
    const ctx = document.getElementById('courseAudienceChart').getContext('2d');

    if (courseAudienceChart) {
        courseAudienceChart.destroy();
    }

    courseAudienceChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: audienceData.map(item => item.targetAudience || 'General'),
            datasets: [{
                data: audienceData.map(item => item.count),
                backgroundColor: PIE_COLORS,
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function updateMonthlyRegistrationsChart(registrationData) {
    const ctx = document.getElementById('monthlyRegistrationsChart').getContext('2d');

    if (monthlyRegistrationsChart) {
        monthlyRegistrationsChart.destroy();
    }

    monthlyRegistrationsChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: registrationData.map(item => {
                const [year, month] = item.month.split('-');
                return new Date(year, month - 1).toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
            }),
            datasets: [{
                label: 'New Users',
                data: registrationData.map(item => item.count),
                backgroundColor: CHART_COLORS.success,
                borderColor: CHART_COLORS.success,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
}

function updateTopConsultantsTable(consultants) {
    const tbody = $('#topConsultantsTable tbody');
    tbody.empty();

    consultants.forEach(consultant => {
        const row = `
            <tr>
                <td>${consultant.consultantName}</td>
                <td>${consultant.expertise || 'N/A'}</td>
                <td><span class="badge bg-primary">${consultant.appointmentCount}</span></td>
            </tr>
        `;
        tbody.append(row);
    });
}

function updateTopCoursesTable(courses) {
    const tbody = $('#topCoursesTable tbody');
    tbody.empty();

    courses.forEach(course => {
        const row = `
            <tr>
                <td>${course.title}</td>
                <td>${course.targetAudience || 'General'}</td>
                <td><span class="badge bg-success">${course.enrollmentCount}</span></td>
            </tr>
        `;
        tbody.append(row);
    });
}

function updateUpcomingAppointmentsTable(appointments) {
    const tbody = $('#upcomingAppointmentsTable tbody');
    tbody.empty();

    if (appointments.length === 0) {
        tbody.append('<tr><td colspan="4" class="text-center text-muted">No upcoming appointments</td></tr>');
        return;
    }

    appointments.forEach(appointment => {
        const appointmentDate = new Date(appointment.appointmentDate);
        const formattedDate = appointmentDate.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });

        const statusClass = getStatusBadgeClass(appointment.status);

        const row = `
            <tr>
                <td>${formattedDate}</td>
                <td>${appointment.userName}</td>
                <td>${appointment.consultantName}</td>
                <td><span class="badge ${statusClass}">${appointment.status}</span></td>
            </tr>
        `;
        tbody.append(row);
    });
}

function updatePopularSurveysTable(surveys) {
    const tbody = $('#popularSurveysTable tbody');
    tbody.empty();

    surveys.forEach(survey => {
        const responseRate = Math.min(95, Math.max(10, survey.participantCount * 2));

        const row = `
            <tr>
                <td>${survey.name}</td>
                <td><span class="badge bg-info">${survey.participantCount}</span></td>
                <td>
                    <div class="progress" style="height: 20px;">
                        <div class="progress-bar" role="progressbar" style="width: ${responseRate}%">
                            ${responseRate}%
                        </div>
                    </div>
                </td>
            </tr>
        `;
        tbody.append(row);
    });
}

function getStatusBadgeClass(status) {
    switch (status.toLowerCase()) {
        case 'pending': return 'bg-warning';
        case 'confirmed': return 'bg-info';
        case 'completed': return 'bg-success';
        case 'cancelled': return 'bg-danger';
        default: return 'bg-secondary';
    }
}

function showLoading(show) {
    if (show) {
        $('#loadingSpinner').show();
        $('#summaryCards').hide();
    } else {
        $('#loadingSpinner').hide();
        $('#summaryCards').show();
    }
}

function showAlert(message, type = 'info') {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    $('.container-fluid').prepend(alertHtml);

    setTimeout(() => {
        $('.alert').fadeOut();
    }, 5000);
}

async function makeApiCall(endpoint, options = {}) {
    const token = window.USER_TOKEN;

    const defaultOptions = {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': token ? `Bearer ${token}` : ''
        }
    };

    const finalOptions = { ...defaultOptions, ...options };

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, finalOptions);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data;
    } catch (error) {
        throw error;
    }
}

function refreshDashboard() {
    showAlert('Đang làm mới dashboard...', 'info');
    loadDashboard();
}

async function exportDashboardReport() {
    try {
        showAlert('Đang tạo báo cáo Dashboard...', 'info');

        const token = window.USER_TOKEN;

        const response = await fetch(`${API_BASE_URL}/ExcelExport/dashboard`, {
            method: 'GET',
            headers: {
                'Authorization': token ? `Bearer ${token}` : '',
                'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const contentDisposition = response.headers.get('Content-Disposition');
        let filename = `Dashboard_Report_${new Date().toISOString().split('T')[0]}.xlsx`;

        if (contentDisposition) {
            const filenameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
            if (filenameMatch && filenameMatch[1]) {
                filename = filenameMatch[1].replace(/['"]/g, '');
            }
        }

        const blob = await response.blob();
        downloadBlob(blob, filename);

        showAlert('Báo cáo Dashboard đã được tải xuống thành công!', 'success');

    } catch (error) {
        showAlert(`Lỗi khi xuất báo cáo Dashboard: ${error.message}`, 'danger');
    }
}

function downloadBlob(blob, filename) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.style.display = 'none';
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
}

setInterval(() => {
    loadDashboard();
}, 5 * 60 * 1000);
