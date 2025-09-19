const preview = document.getElementById('avatar-preview');
const options = document.querySelectorAll('.avatar-option');
const saveBtn = document.getElementById('save-btn');

window.addEventListener('DOMContentLoaded', () => {
  const savedAvatar = localStorage.getItem('epicqrizzz-avatar');
  if (savedAvatar) {
    preview.textContent = savedAvatar;
  }
});

options.forEach(option => {
  option.addEventListener('click', () => {
    preview.textContent = option.textContent;
  });
});

saveBtn.addEventListener('click', () => {
  const selectedAvatar = preview.textContent;
  localStorage.setItem('epicqrizzz-avatar', selectedAvatar);
  alert('Profiel opgeslagen!');
});
