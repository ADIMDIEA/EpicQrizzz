// assets/js/logo.js
document.addEventListener("DOMContentLoaded", function() {
  const header = document.createElement("div");
  header.className = "text-center my-3";
  header.innerHTML = `
    <img src="assets/img/logo.png" alt="EpicQrizzz Logo" style="max-width: 150px;">
  `;
  document.body.prepend(header);
});
