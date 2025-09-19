const backendBase = "http://joost.assenbergh.nl:5292/api/User";

const loginTab = document.getElementById('login-tab');
const registerTab = document.getElementById('register-tab');
const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');

// Switch tabs
loginTab.addEventListener('click', () => {
  loginTab.classList.add('active');
  registerTab.classList.remove('active');
  loginForm.classList.add('active');
  registerForm.classList.remove('active');
});

registerTab.addEventListener('click', () => {
  registerTab.classList.add('active');
  loginTab.classList.remove('active');
  registerForm.classList.add('active');
  loginForm.classList.remove('active');
});

// UUID generator
function generateId() {
  if (crypto.randomUUID) return crypto.randomUUID();
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = Math.random() * 16 | 0;
    const v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

// SHA-256 hashing
async function hashPassword(password) {
  const encoder = new TextEncoder();
  const data = encoder.encode(password);
  const hashBuffer = await crypto.subtle.digest('SHA-256', data);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
}

// Login
loginForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  const username = document.getElementById('login-username').value;
  const password = document.getElementById('login-password').value;

  try {
    const hashedPassword = await hashPassword(password);
    const res = await fetch(`${backendBase}/Login/${encodeURIComponent(username)}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ enteredPassword: hashedPassword })
    });

    if (res.ok) {
      const data = await res.json();
      alert('Welkom! Je account-ID is: ' + data.id);
      window.location.href = 'home.html';
    } else {
      alert('Login mislukt: ' + res.statusText);
    }
  } catch (err) {
    console.error(err);
    alert('Er ging iets mis met inloggen');
  }
});

// Register
registerForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  const username = document.getElementById('register-username').value;
  const password = document.getElementById('register-password').value;
  const password2 = document.getElementById('register-password2').value;

  if (password !== password2) {
    alert('Wachtwoorden komen niet overeen!');
    return;
  }

  try {
    const id = generateId();
    const hashedPassword = await hashPassword(password);
    const res = await fetch(`${backendBase}/CreateAccount`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Id: id, Name: username, Password: hashedPassword })
    });

    if (res.ok) {
      alert('Account aangemaakt! Je ID is: ' + id);
      loginTab.click();
    } else {
      alert('Registratie mislukt: ' + res.statusText);
    }
  } catch (err) {
    console.error(err);
    alert('Er ging iets mis met registreren');
  }
});

// Optional: fetch all users for testing
async function fetchAllUsers() {
  try {
    const res = await fetch(`${backendBase}/GetAll`);
    if (res.ok) {
      const users = await res.json();
      console.log('All users:', users);
    }
  } catch (err) {
    console.error('Failed to fetch users:', err);
  }
}

// fetchAllUsers(); // uncomment to debug
