class AuthHelper {
    constructor() {
        this.currentUser = window.CURRENT_USER_ID || null;
        this.userToken = window.USER_TOKEN || null;
        this.userRole = window.USER_ROLE || 'Guest';
    }

    isAuthenticated() {
        return this.currentUser !== null && this.userToken !== null;
    }

    isAdmin() {
        return this.userRole === 'Admin';
    }

    isManager() {
        return this.userRole === 'Manager';
    }

    isStaff() {
        return this.userRole === 'Staff';
    }

    isConsultant() {
        return this.userRole === 'Consultant';
    }

    isMember() {
        return this.userRole === 'Member';
    }

    canManageCourses() {
        return ['Staff', 'Manager', 'Admin'].includes(this.userRole);
    }

    canManageSurveys() {
        return ['Staff', 'Manager', 'Admin'].includes(this.userRole);
    }

    canManageUsers() {
        return this.userRole === 'Admin';
    }

    canViewReports() {
        return ['Manager', 'Admin'].includes(this.userRole);
    }

    canRegisterCourses() {
        return this.isAuthenticated() && this.userRole !== 'Guest';
    }

    canTakeSurveys() {
        return this.isAuthenticated() && this.userRole !== 'Guest';
    }

    canBookAppointments() {
        return this.isAuthenticated() && this.userRole !== 'Guest';
    }

    // Show/hide elements based on permissions
    toggleElementsByRole(selector, allowedRoles) {
        const elements = document.querySelectorAll(selector);
        const isAllowed = allowedRoles.includes(this.userRole);

        elements.forEach(element => {
            if (isAllowed) {
                element.style.display = '';
            } else {
                element.style.display = 'none';
            }
        });
    }

    // Add auth headers to API requests
    getAuthHeaders() {
        return {
            'Authorization': `Bearer ${this.userToken}`,
            'Content-Type': 'application/json'
        };
    }

    // Check ownership for resources
    isOwner(resourceUserId) {
        return this.currentUser === resourceUserId;
    }

    canAccessResource(resourceUserId, requiredRoles = []) {
        return this.isOwner(resourceUserId) ||
            requiredRoles.includes(this.userRole) ||
            this.isAdmin();
    }
}

const authHelper = new AuthHelper();
