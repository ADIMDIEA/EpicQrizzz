// Toon opgeslagen avatar uit localStorage
const avatarDisplay = document.getElementById('avatar-display');
const savedAvatar = localStorage.getItem('epicqrizzz-avatar');
if (savedAvatar) {
  avatarDisplay.textContent = savedAvatar;
}

// Uitlogknop
const logoutBtn = document.getElementById('logout-btn');
logoutBtn.addEventListener('click', () => {
  localStorage.removeItem('epicqrizzz-avatar');
  window.location.href = 'login.html';
});
