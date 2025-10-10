const backendBase = "http://joost.assenbergh.nl:5292/api/User";

// SHA-256 hashing
async function hashPassword(password) {
  const encoder = new TextEncoder();
  const data = encoder.encode(password);
  const hashBuffer = await crypto.subtle.digest("SHA-256", data);
  const hashArray = Array.from(new Uint8Array(hashBuffer));
  return hashArray.map(b => b.toString(16).padStart(2, "0")).join("");
}

// Login
const loginForm = document.getElementById("login-form");
loginForm.addEventListener("submit", async (e) => {
  e.preventDefault();
  const username = document.getElementById("login-username").value;
  const password = document.getElementById("login-password").value;

  try {
    const hashedPassword = await hashPassword(password);
    const res = await fetch(`${backendBase}/Login/${encodeURIComponent(username)}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ enteredPassword: hashedPassword })
    });

    if (res.ok) {
      const data = await res.json();
      sessionStorage.setItem("epicqrizzz-userId", data.id);
      sessionStorage.setItem("epicqrizzz-username", username);
      const response = await fetch(`http://joost.assenbergh.nl:5295/api/Inventory/CheckEquipped/${data.id}`, {
        method: "GET",
        headers: { "Content-Type": "application/json" }
      }); 
      if (response.ok) {
        const data2 = await response.json();
        console.log(data2)
        sessionStorage.setItem("selectedAvatarId", data2.itemId);
      } else {
        alert("Probleem met het inladen van je avatar.")
      }
      window.location.href = "home.html";
    } else {
      alert("Login mislukt: " + res.statusText);
    }
    

  } catch (err) {
    console.error(err);
    alert("Er ging iets mis met inloggen");
  }
});
