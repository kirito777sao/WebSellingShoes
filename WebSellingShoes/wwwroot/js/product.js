console.log("product.js loaded");
//hướng dẫn chọn size khi ấn vào link footer
// script.js
document.addEventListener("DOMContentLoaded", function () {
    var modal = document.getElementById("sizeGuideModal");
    var btns = document.querySelectorAll("[id^='sizeGuideLink']"); // Chọn tất cả các phần tử có id bắt đầu bằng 'sizeGuideLink'
    var span = document.getElementsByClassName("close")[0];
    var iframe = document.getElementById("sizeGuideIframe");

    // Khi nhấp vào liên kết
    btns.forEach(btn => {
        btn.onclick = function (event) {
            event.preventDefault();
            modal.style.display = "block";
            iframe.src = "/Views/Shared/Chonsize"; // Đường dẫn chính xác đến file Chonsize.cshtml
            document.body.style.overflow = "hidden"; // Tắt cuộn chuột
        };
    });

    // Khi nhấp vào nút đóng
    span.onclick = function () {
        modal.style.display = "none";
        document.body.style.overflow = ""; // Bật lại cuộn chuột
    };

    // Khi nhấp ngoài modal
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
            document.body.style.overflow = ""; // Bật lại cuộn chuột
        }
    };
});

// zoom ảnh theo chuột ở details sản phẩm
document.addEventListener("DOMContentLoaded", function () {
    const mainImageContainer = document.querySelector(".main-image-container");
    mainImageContainer.addEventListener("mousemove", (e) => {
        const activeItem = mainImageContainer.querySelector(".carousel-item.active img");
        if (!activeItem) return;

        const rect = mainImageContainer.getBoundingClientRect();
        const x = ((e.clientX - rect.left) / rect.width) * 100;
        const y = ((e.clientY - rect.top) / rect.height) * 100;

        activeItem.style.transformOrigin = `${x}% ${y}%`;
        activeItem.style.transform = "scale(2)";
    });

    mainImageContainer.addEventListener("mouseleave", () => {
        const activeItem = mainImageContainer.querySelector(".carousel-item.active img");
        if (!activeItem) return;

        activeItem.style.transformOrigin = "center center";
        activeItem.style.transform = "scale(1)";
    });
});

//hướng dẫn chọn size khi ấn vào link footer
// script.js
document.addEventListener("DOMContentLoaded", function () {
    var modal = document.getElementById("sizeGuideModal");
    var btns = document.querySelectorAll("[id^='sizeGuideLink']"); // Chọn tất cả các phần tử có id bắt đầu bằng 'sizeGuideLink'
    var span = document.getElementsByClassName("close")[0];
    var iframe = document.getElementById("sizeGuideIframe");

    // Khi nhấp vào liên kết
    btns.forEach(btn => {
        btn.onclick = function (event) {
            event.preventDefault();
            modal.style.display = "block";
            iframe.src = "/Chonsize/SizeGuide"; // Đường dẫn đến controller action
            document.body.style.overflow = "hidden"; // Tắt cuộn chuột
        };
    });

    // Khi nhấp vào nút đóng
    span.onclick = function () {
        modal.style.display = "none";
        document.body.style.overflow = ""; // Bật lại cuộn chuột
    };

    // Khi nhấp ngoài modal
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
            document.body.style.overflow = ""; // Bật lại cuộn chuột
        }
    };
});

document.addEventListener("DOMContentLoaded", function () {
    const mainImageContainer = document.querySelector(".main-image-container");

    mainImageContainer.addEventListener("mousemove", (e) => {
        const activeItem = mainImageContainer.querySelector(".carousel-item.active img");
        if (!activeItem) return;

        // Tính toán vị trí chuột và thiết lập transform-origin
        const rect = mainImageContainer.getBoundingClientRect();
        const x = ((e.clientX - rect.left) / rect.width) * 100;
        const y = ((e.clientY - rect.top) / rect.height) * 100;
        
        // Chỉ set transform-origin trong JS (cần thiết vì phụ thuộc vào vị trí chuột)
        activeItem.style.transformOrigin = `${x}% ${y}%`;
        
        // Thêm class để kích hoạt hiệu ứng zoom từ CSS
        activeItem.classList.add("zoom-active");
    });

    mainImageContainer.addEventListener("mouseleave", () => {
        const activeItem = mainImageContainer.querySelector(".carousel-item.active img");
        if (!activeItem) return;
        
        // Reset transform-origin và xóa class khi rời chuột
        activeItem.style.transformOrigin = "center center";
        activeItem.classList.remove("zoom-active");
    });

    // Xử lý thumbnail active
    document.querySelectorAll('.thumbnail-img').forEach(thumb => {
        thumb.addEventListener('click', function() {
            document.querySelectorAll('.thumbnail-img').forEach(t => 
                t.classList.remove('active'));
            this.classList.add('active');
        });
    });
});


// Back to Top Button
$(document).ready(function () {
    // Hiển thị hoặc ẩn nút "Back to Top" khi cuộn
    $(window).scroll(function () {
        if ($(this).scrollTop() > 300) {
            $(".back-to-top").addClass("show");
        } else {
            $(".back-to-top").removeClass("show");
        }
    });

    // Cuộn mượt về đầu trang khi nhấn vào nút
    $(".back-to-top").click(function () {
        $("html, body").animate({ scrollTop: 0 }, 1500, "easeInOutExpo");
        return false;
    });
});