console.log("Global script loaded");
document.addEventListener("DOMContentLoaded", function () {
    // Xử lý khi ấn Enter trong ô tìm kiếm
    let input = document.getElementById("search1");
    if (input) {
        input.addEventListener("keypress", function (e) {
            if (e.key === "Enter") {
                searchRedirect();
            }
        });
    }

    // Hàm chuyển hướng khi tìm kiếm
    window.searchRedirect = function () {
        let query = document.getElementById("search1").value;
        if (query.trim() !== "") {
            window.location.href = "/Search?query=" + encodeURIComponent(query);
        }
    }
    // scrip flashsale

});




