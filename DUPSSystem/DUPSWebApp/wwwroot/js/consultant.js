let currentPage = 1;
const pageSize = 10;
let currentSearchTerm = '';

function showToast(type, message) {
    const toastId = type === 'success' ? 'successToast' : 'errorToast';
    const messageId = type === 'success' ? 'successMessage' : 'errorMessage';

    document.getElementById(messageId).textContent = message;

    const toast = new bootstrap.Toast(document.getElementById(toastId));
    toast.show();
}

async function loadConsultantProfile() {
    try {
        const response = await fetch('/Consultant/GetMyProfile', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                displayProfile(result.data);
            } else {
                showToast('error', result.message || 'Không thể tải thông tin hồ sơ');
            }
        } else {
            showToast('error', 'Không thể tải thông tin hồ sơ');
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi tải hồ sơ');
    }
}

function displayProfile(data) {
    const userName = data.User?.FullName || data.user?.fullName || '';
    const userEmail = data.User?.Email || data.user?.email || '';
    const userPhone = data.User?.Phone || data.user?.phone || '';

    document.getElementById('displayFullName').textContent = userName || '--';
    document.getElementById('displayEmail').textContent = userEmail || '--';
    document.getElementById('displayPhone').textContent = userPhone || '--';

    document.getElementById('displayQualification').textContent = data.qualification || data.Qualification || '--';
    document.getElementById('displayExpertise').textContent = data.expertise || data.Expertise || '--';
    document.getElementById('displayWorkSchedule').textContent = data.workSchedule || data.WorkSchedule || '--';
    document.getElementById('displayBio').textContent = data.bio || data.Bio || '--';

    document.getElementById('qualification').value = data.qualification || data.Qualification || '';
    document.getElementById('expertise').value = data.expertise || data.Expertise || '';
    document.getElementById('workSchedule').value = data.workSchedule || data.WorkSchedule || '';
    document.getElementById('bio').value = data.bio || data.Bio || '';
}

function showEditForm() {
    document.getElementById('profileInfo').style.display = 'none';
    document.getElementById('editForm').style.display = 'block';
}

function cancelEdit() {
    document.getElementById('profileInfo').style.display = 'block';
    document.getElementById('editForm').style.display = 'none';
}

async function updateProfile() {
    try {
        const formData = {
            qualification: document.getElementById('qualification').value,
            expertise: document.getElementById('expertise').value,
            workSchedule: document.getElementById('workSchedule').value,
            bio: document.getElementById('bio').value
        };

        const response = await fetch('/Consultant/UpdateProfile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        const result = await response.json();

        if (result.success) {
            showToast('success', result.message);
            cancelEdit();
            loadConsultantProfile();
        } else {
            showToast('error', result.message);
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi cập nhật hồ sơ');
    }
}

async function loadConsultants(page = 1, search = '') {
    try {
        document.getElementById('loadingSpinner').style.display = 'block';

        const response = await fetch(`/Consultant/GetConsultants?page=${page}&pageSize=${pageSize}&search=${encodeURIComponent(search)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (result.success) {
            const data = JSON.parse(result.data);
            displayConsultants(data.value || []);
            updatePagination(data['@odata.count'] || 0, page);
        } else {
            showToast('error', result.message);
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi tải danh sách tư vấn viên');
    } finally {
        document.getElementById('loadingSpinner').style.display = 'none';
    }
}

async function loadAvailableUsers() {
    try {
        const response = await fetch('/Consultant/GetAvailableUsers', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const result = await response.json();
            if (result.success) {
                const userSelect = document.getElementById('createUserId');
                userSelect.innerHTML = '<option value="">-- Chọn người dùng --</option>';

                result.data.forEach(user => {
                    const option = document.createElement('option');
                    option.value = user.UserId;
                    option.textContent = `${user.FullName} (${user.Email})`;
                    userSelect.appendChild(option);
                });
            }
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi tải danh sách người dùng');
    }
}

function displayConsultants(consultants) {
    const tbody = document.getElementById('consultantTableBody');

    if (consultants.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center text-muted">
                    <i class="fas fa-info-circle me-2"></i>Không có dữ liệu
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = consultants.map(consultant => {
        const fullName = consultant.User?.FullName || consultant.user?.fullName || '--';
        const email = consultant.User?.Email || consultant.user?.email || '--';
        const phone = consultant.User?.Phone || consultant.user?.phone || '--';
        const expertise = consultant.Expertise || consultant.expertise || '--';
        const isActive = consultant.User?.IsActive !== false;
        const consultantId = consultant.ConsultantId || consultant.consultantId;

        return `
            <tr>
                <td>${consultantId}</td>
                <td>${fullName}</td>
                <td>${email}</td>
                <td>${phone}</td>
                <td>${expertise}</td>
                <td>
                    <span class="badge ${isActive ? 'bg-success' : 'bg-danger'}">
                        ${isActive ? 'Hoạt động' : 'Không hoạt động'}
                    </span>
                </td>
                <td class="text-center">
                    <div class="btn-group" role="group">
                        <button type="button" class="btn btn-sm btn-outline-info" 
                                onclick="viewConsultantDetails(${consultantId})" 
                                title="Xem chi tiết">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-warning" 
                                onclick="editConsultant(${consultantId})" 
                                title="Chỉnh sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-outline-danger" 
                                onclick="deleteConsultant(${consultantId}, '${fullName}')" 
                                title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }).join('');
}

function updatePagination(totalCount, currentPageNum) {
    const totalPages = Math.ceil(totalCount / pageSize);
    const paginationNav = document.getElementById('paginationNav');
    const paginationList = document.getElementById('paginationList');

    if (totalPages <= 1) {
        paginationNav.style.display = 'none';
        return;
    }

    paginationNav.style.display = 'block';
    currentPage = currentPageNum;

    let paginationHTML = '';

    if (currentPage > 1) {
        paginationHTML += `
            <li class="page-item">
                <a class="page-link" href="#" onclick="loadConsultants(${currentPage - 1}, '${currentSearchTerm}')">
                    <i class="fas fa-chevron-left"></i>
                </a>
            </li>
        `;
    }

    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    for (let i = startPage; i <= endPage; i++) {
        paginationHTML += `
            <li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="loadConsultants(${i}, '${currentSearchTerm}')">${i}</a>
            </li>
        `;
    }

    if (currentPage < totalPages) {
        paginationHTML += `
            <li class="page-item">
                <a class="page-link" href="#" onclick="loadConsultants(${currentPage + 1}, '${currentSearchTerm}')">
                    <i class="fas fa-chevron-right"></i>
                </a>
            </li>
        `;
    }

    paginationList.innerHTML = paginationHTML;
}

function searchConsultants() {
    const searchTerm = document.getElementById('searchInput').value.trim();
    currentSearchTerm = searchTerm;
    currentPage = 1;
    loadConsultants(1, searchTerm);
}

function clearSearch() {
    document.getElementById('searchInput').value = '';
    currentSearchTerm = '';
    currentPage = 1;
    loadConsultants(1, '');
}

async function createConsultant() {
    try {
        const formData = {
            userId: parseInt(document.getElementById('createUserId').value),
            qualification: document.getElementById('createQualification').value,
            expertise: document.getElementById('createExpertise').value,
            workSchedule: document.getElementById('createWorkSchedule').value,
            bio: document.getElementById('createBio').value
        };

        if (!formData.userId) {
            showToast('error', 'Vui lòng chọn người dùng');
            return;
        }

        const response = await fetch('/Consultant/CreateConsultant', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        const result = await response.json();

        if (result.success) {
            showToast('success', result.message);
            bootstrap.Modal.getInstance(document.getElementById('createConsultantModal')).hide();
            document.getElementById('createConsultantForm').reset();
            loadConsultants(currentPage, currentSearchTerm);
        } else {
            showToast('error', result.message);
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi tạo tư vấn viên');
    }
}

async function editConsultant(consultantId) {
    try {
        const response = await fetch(`/Consultant/GetConsultantDetails/${consultantId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const result = await response.json();

            if (result.success) {
                const consultant = result.data;

                document.getElementById('editConsultantId').value = consultant.ConsultantId || consultant.consultantId;
                document.getElementById('editQualification').value = consultant.Qualification || consultant.qualification || '';
                document.getElementById('editExpertise').value = consultant.Expertise || consultant.expertise || '';
                document.getElementById('editWorkSchedule').value = consultant.WorkSchedule || consultant.workSchedule || '';
                document.getElementById('editBio').value = consultant.Bio || consultant.bio || '';

                bootstrap.Modal.getOrCreateInstance(document.getElementById('editConsultantModal')).show();
            } else {
                showToast('error', result.message);
            }
        } else {
            showToast('error', 'Không thể tải thông tin tư vấn viên');
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi tải thông tin');
    }
}

async function updateConsultant() {
    try {
        const formData = {
            consultantId: parseInt(document.getElementById('editConsultantId').value),
            qualification: document.getElementById('editQualification').value,
            expertise: document.getElementById('editExpertise').value,
            workSchedule: document.getElementById('editWorkSchedule').value,
            bio: document.getElementById('editBio').value
        };

        const response = await fetch('/Consultant/UpdateConsultant', {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        const result = await response.json();

        if (result.success) {
            showToast('success', result.message);
            bootstrap.Modal.getInstance(document.getElementById('editConsultantModal')).hide();
            loadConsultants(currentPage, currentSearchTerm);
        } else {
            showToast('error', result.message);
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi cập nhật tư vấn viên');
    }
}

function deleteConsultant(consultantId, consultantName) {
    document.getElementById('deleteConsultantId').value = consultantId;
    document.getElementById('deleteConsultantName').textContent = consultantName;
    bootstrap.Modal.getOrCreateInstance(document.getElementById('deleteConfirmModal')).show();
}

async function confirmDeleteConsultant() {
    try {
        const consultantId = document.getElementById('deleteConsultantId').value;

        const response = await fetch(`/Consultant/DeleteConsultant/${consultantId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const result = await response.json();

        if (result.success) {
            showToast('success', result.message);
            bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal')).hide();
            loadConsultants(currentPage, currentSearchTerm);
        } else {
            showToast('error', result.message);
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi xóa tư vấn viên');
    }
}

async function viewConsultantDetails(consultantId) {
    try {
        const response = await fetch(`/Consultant/GetConsultantDetails/${consultantId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (response.ok) {
            const result = await response.json();

            if (result.success) {
                const consultant = result.data;

                const fullName = consultant.User?.FullName || consultant.user?.fullName || '--';
                const email = consultant.User?.Email || consultant.user?.email || '--';
                const phone = consultant.User?.Phone || consultant.user?.phone || '--';
                const qualification = consultant.Qualification || consultant.qualification || '--';
                const expertise = consultant.Expertise || consultant.expertise || '--';
                const workSchedule = consultant.WorkSchedule || consultant.workSchedule || '--';
                const bio = consultant.Bio || consultant.bio || '--';
                const isActive = consultant.User?.IsActive !== false;

                document.getElementById('viewFullName').textContent = fullName;
                document.getElementById('viewEmail').textContent = email;
                document.getElementById('viewPhone').textContent = phone;
                document.getElementById('viewQualification').textContent = qualification;
                document.getElementById('viewExpertise').textContent = expertise;
                document.getElementById('viewWorkSchedule').textContent = workSchedule;
                document.getElementById('viewBio').textContent = bio;

                const statusBadge = isActive
                    ? '<span class="badge bg-success">Hoạt động</span>'
                    : '<span class="badge bg-danger">Không hoạt động</span>';
                document.getElementById('viewStatus').innerHTML = statusBadge;

                bootstrap.Modal.getOrCreateInstance(document.getElementById('viewDetailsModal')).show();
            } else {
                showToast('error', result.message);
            }
        } else {
            showToast('error', 'Không thể tải thông tin tư vấn viên');
        }
    } catch (error) {
        showToast('error', 'Đã xảy ra lỗi khi tải thông tin chi tiết');
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const createModal = document.getElementById('createConsultantModal');
    if (createModal) {
        createModal.addEventListener('show.bs.modal', function () {
            loadAvailableUsers();
        });
    }
}); 