document.addEventListener("DOMContentLoaded", () => {
  const avatarDisplay = document.getElementById("avatar-display");
  const savedAvatar = localStorage.getItem("epicqrizzz-avatar");
  if (savedAvatar) avatarDisplay.textContent = savedAvatar;

  const logoutBtn = document.getElementById("logout-btn");
  logoutBtn.addEventListener("click", () => {
    localStorage.removeItem("epicqrizzz-avatar");
    localStorage.removeItem("epicqrizzz-user");
    window.location.href = "index.html";
  });
});
