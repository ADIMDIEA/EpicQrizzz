const backendBase = "http://joost.assenbergh.nl:5292/api/User";

// UUID generator
function generateId() {
  if (crypto.randomUUID) return crypto.randomUUID();
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, c => {
    const r = Math.random() * 16 | 0;
    const v = c === "x" ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

// SHA-256 hashing
async function hashPassword(password) {
  const encoder = new TextEncoder();
  const data = encoder.encode(password);
  const hashBuffer = await crypto.subtle.digest("SHA-256", data);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  return hashArray.map(b => b.toString(16).padStart(2, "0")).join("");
}

// Register
const registerForm = document.getElementById("register-form");
registerForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const username = document.getElementById("register-username").value;
  const password = document.getElementById("register-password").value;
  const password2 = document.getElementById("register-password2").value;

  if (password !== password2) {
    alert("Wachtwoorden komen niet overeen!");
    return;
  }

  try {
    const id = generateId();
    const hashedPassword = await hashPassword(password);

    const res = await fetch(`${backendBase}/CreateAccount`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ Id: id, Name: username, Password: hashedPassword })
    });

    if (res.ok) {
      alert("Account aangemaakt! Je kunt nu inloggen.");
      window.location.href = "login.html";
    } else {
      alert("Registratie mislukt: " + res.statusText);
    }
  } catch (err) {
    console.error(err);
    alert("Er ging iets mis met registreren");
  }
});
