document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("globalSearchInput");
    const searchBtn = document.getElementById("globalSearchBtn");

    // Tạo ô thông báo
    const notification = document.createElement("div");
    notification.id = "searchNotification";
    notification.style.position = "fixed";
    notification.style.bottom = "20px";
    notification.style.left = "50%";
    notification.style.transform = "translateX(-50%)";
    notification.style.padding = "10px 20px";
    notification.style.backgroundColor = "rgba(255,0,0,0.8)";
    notification.style.color = "#fff";
    notification.style.fontWeight = "bold";
    notification.style.borderRadius = "5px";
    notification.style.opacity = "0";
    notification.style.transition = "opacity 0.5s";
    notification.style.zIndex = "9999";
    document.body.appendChild(notification);

    function showNotification(message) {
        notification.textContent = message;
        notification.style.opacity = "1";
        setTimeout(() => {
            notification.style.opacity = "0";
        }, 5000); // mờ dần trong 5s
    }

    function removeHighlights() {
        document.querySelectorAll("td span.highlight").forEach(span => {
            const parent = span.parentNode;
            parent.replaceChild(document.createTextNode(span.textContent), span);
            parent.normalize();
        });
    }

    function highlightText(cell, text) {
        const regex = new RegExp(`(${text})`, "gi");
        cell.innerHTML = cell.textContent.replace(regex, "<span class='highlight'>$1</span>");
    }

    function filterAndHighlightTables() {
        const filter = searchInput.value.trim().toLowerCase();
        removeHighlights();

        if (!filter) {
            document.querySelectorAll("table tr").forEach(row => row.style.display = "");
            return;
        }

        const tables = document.querySelectorAll("table");
        let firstMatch = null;
        let hasMatch = false;

        tables.forEach(table => {
            const rows = table.getElementsByTagName("tr");

            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                const cells = row.getElementsByTagName("td");
                let rowMatch = false;

                for (let j = 0; j < cells.length; j++) {
                    const cell = cells[j];
                    if (cell.textContent.toLowerCase().includes(filter)) {
                        highlightText(cell, filter);
                        rowMatch = true;
                        if (!firstMatch) firstMatch = cell;
                        hasMatch = true;
                    }
                }

                row.style.display = rowMatch ? "" : "none";
            }
        });

        if (!hasMatch) {
            showNotification("Không tìm thấy kết quả nào!");
        }

        if (firstMatch) {
            firstMatch.scrollIntoView({ behavior: "smooth", block: "center" });
        }
    }

    searchBtn.addEventListener("click", filterAndHighlightTables);

    searchInput.addEventListener("keyup", function (e) {
        if (e.key === "Enter") filterAndHighlightTables();
    });
});
