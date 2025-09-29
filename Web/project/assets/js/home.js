

const avatarDisplay = document.getElementById("avatar-display");
const adminBtn = document.getElementById("admin-btn");
const logoutBtn = document.getElementById("logout-btn");

// Huidige gebruiker ophalen (sessie/cookie uit backend)
async function loadUser() {
  try {

    if (!sessionStorage.getItem("epicqrizzz-userId")) {
      // niet ingelogd, terug naar login
      window.location.href = "login.html";
      return;
    }

  } catch (err) {
    console.error("Kon gebruiker niet ophalen:", err);
    window.location.href = "login.html";
  }
}

// Uitloggen
logoutBtn.addEventListener("click", async () => {
  try {
    await fetch(`${backendBase}/Logout`, {
      method: "POST",
      credentials: "include"
    });
  } catch (err) {
    console.error("Fout bij uitloggen:", err);
  }
  window.location.href = "login.html";
});

loadUser();
