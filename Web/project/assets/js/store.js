document.querySelectorAll('.buyPackBtn').forEach(button => { 
    button.addEventListener('click', async () => {
        const packId = button.dataset.packId;
        const userId = sessionStorage.getItem('epicqrizzz-userId');

        if (!userId) {
            alert('Vul eerst een User ID in!');
            return;
        }

        try {
            const buyResponse = await fetch(`http://joost.assenbergh.nl:5294/api/store/BuyPack/${packId}/${userId}`, {
                method: 'POST'
            });

            if (buyResponse.ok) {
                const buyData = await buyResponse.json();
                alert(`Pack gekocht! Item ID: ${buyData.itemId}`);
            } else if (buyResponse.status === 601) {
                alert('Je hebt niet genoeg munten om dit pack te kopen.');
            } else {
                const err = await buyResponse.text();
                alert('Fout bij kopen van pack: ' + err);
            }

        } catch (error) {
            console.error(error);
            alert('Er is een fout opgetreden bij de API-call');
        }
    });
});
