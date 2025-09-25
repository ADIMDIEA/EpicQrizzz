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

// Helper om meldingen te tonen
function showMessage(message, type = "danger", autoHide = false) {
  const messageBox = document.getElementById("register-message");
  messageBox.innerHTML = `
    <div class="alert alert-${type} alert-dismissible fade show" role="alert">
      ${message}
      <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Sluiten"></button>
    </div>
  `;

  if (autoHide) {
    setTimeout(() => {
      const alertEl = messageBox.querySelector(".alert");
      if (alertEl) {
        alertEl.classList.remove("show");
        alertEl.classList.add("fade");
        setTimeout(() => (messageBox.innerHTML = ""), 500);
      }
    }, 3000);
  }
}

// Register
const registerForm = document.getElementById("register-form");
registerForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const username = document.getElementById("register-username").value;
  const password = document.getElementById("register-password").value;
  const password2 = document.getElementById("register-password2").value;

  // Validatie
  if (username.trim() === "") {
    showMessage("Gebruikersnaam mag niet leeg zijn!");
    return;
  }
  if (password.length < 6) {
    showMessage("Wachtwoord moet minstens 6 tekens lang zijn!");
    return;
  }
  if (password !== password2) {
    showMessage("Wachtwoorden komen niet overeen!");
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
      showMessage("✅ Account succesvol aangemaakt! Je wordt doorgestuurd naar de loginpagina...", "success", true);

      setTimeout(() => {
        window.location.href = "login.html";
      }, 3000);
    } else {
      showMessage("Registratie mislukt: " + res.statusText);
    }
  } catch (err) {
    console.error(err);
    showMessage("Er ging iets mis met registreren");
  }
});
