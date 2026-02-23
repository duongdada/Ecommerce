document.addEventListener("DOMContentLoaded", function () {
    /* ===========================
       Sidebar Toggle (Mobile)
    ============================ */
    const menuToggle = document.getElementById('menuToggle');
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');

    if (menuToggle) {
        menuToggle.onclick = () => sidebar.classList.toggle('active');
    }
    if (sidebarToggle) {
        sidebarToggle.onclick = () => sidebar.classList.remove('active');
    }

    document.addEventListener('click', e => {
        if (window.innerWidth < 992) {
            if (!sidebar.contains(e.target) && !menuToggle.contains(e.target)) {
                sidebar.classList.remove('active');
            }
        }
    });

    /* ===========================
       Sidebar Active Menu - FIXED
    ============================ */
    const navItems = document.querySelectorAll(".sidebar-nav .nav-item");

    // Remove all active classes first (clean HTML)
    navItems.forEach(item => item.classList.remove("active"));

    // Get current URL path
    const currentPath = window.location.pathname;

    // Check if any menu matches current URL
    let foundMatch = false;
    navItems.forEach(item => {
        const href = item.getAttribute('href');
        if (href && currentPath.includes(href)) {
            item.classList.add("active");
            foundMatch = true;
        }
    });

    // If no match, activate first item
    if (!foundMatch && navItems.length > 0) {
        navItems[0].classList.add("active");
    }

    // Click handler
    navItems.forEach(item => {
        item.addEventListener("click", function (e) {
            // Remove active from all
            navItems.forEach(i => i.classList.remove("active"));
            // Add active to clicked
            this.classList.add("active");
        });
    });

    /* ===========================
       Confirm Logout
    ============================ */
    const logoutBtn = document.querySelector(".logout-btn");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", function (e) {
            e.preventDefault();
            if (confirm("Bạn chắc chắn muốn đăng xuất chứ?")) {
                window.location.href = "/Admin/Account/Logout";
            }
        });
    }
});