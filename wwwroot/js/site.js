document.addEventListener("DOMContentLoaded", function () {
    const timerCard = document.querySelector(".timer-card");
    if (!timerCard) return;

    const timerDisplay = timerCard.querySelector(".timer-inner");
    const startBtn = timerCard.querySelector(".btn-pill.primary");
    const resetBtn = timerCard.querySelector(".btn-pill.secondary");
    const timerCircle = timerCard.querySelector(".timer-circle");

    // Modal elementi
    const modalBackdrop = document.getElementById("timerModal");
    const minutesInput = document.getElementById("timerMinutesInput");
    const presetButtons = modalBackdrop.querySelectorAll(".timer-preset-btn");
    const modalConfirm = modalBackdrop.querySelector("[data-action='confirm']");
    const modalCancel = modalBackdrop.querySelector("[data-action='cancel']");

    if (!timerDisplay || !startBtn || !resetBtn || !timerCircle || !modalBackdrop) return;

    let timerInterval = null;
    let seconds = 0;
    let totalSeconds = null; // koliko sekund traja trenutna seja
    let running = false;

    function formatTime(sec) {
        const m = Math.floor(sec / 60);
        const s = sec % 60;
        return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
    }

    function updateCircle() {
        if (!totalSeconds || totalSeconds <= 0) {
            timerCircle.style.setProperty("--timer-progress", "0deg");
            return;
        }
        const ratio = Math.min(seconds / totalSeconds, 1);
        const deg = ratio * 360;
        timerCircle.style.setProperty("--timer-progress", `${deg}deg`);
    }

    function showModal() {
        modalBackdrop.classList.add("is-visible");
        // malo delaya da focus lepo dela
        setTimeout(() => {
            minutesInput.focus();
            minutesInput.select();
        }, 10);
    }

    function hideModal() {
        modalBackdrop.classList.remove("is-visible");
    }

    function beginCountdown() {
        running = true;
        startBtn.textContent = "Pause";

        clearInterval(timerInterval);
        timerInterval = setInterval(() => {
            seconds++;

            if (seconds >= totalSeconds) {
                seconds = totalSeconds;
                timerDisplay.textContent = formatTime(seconds);
                updateCircle();

                clearInterval(timerInterval);
                running = false;
                startBtn.textContent = "Start";

                // tu lahko kasneje dodaš custom toast namesto alert
                alert("Čas je potekel! 🎉");
                return;
            }

            timerDisplay.textContent = formatTime(seconds);
            updateCircle();
        }, 1000);
    }

    // Klik na Start / Pause
    startBtn.addEventListener("click", () => {
        if (!running) {
            // če nimamo še izbranega časa, pokaži modal
            if (!totalSeconds) {
                showModal();
            } else {
                beginCountdown();
            }
        } else {
            // Pause
            running = false;
            startBtn.textContent = "Start";
            clearInterval(timerInterval);
        }
    });

    // Reset
    resetBtn.addEventListener("click", () => {
        running = false;
        clearInterval(timerInterval);

        seconds = 0;
        totalSeconds = null; // naslednjič bo spet vprašalo
        timerDisplay.textContent = "00:00";
        startBtn.textContent = "Start";
        timerCircle.style.setProperty("--timer-progress", "0deg");
    });

    // Modal – Confirm
    modalConfirm.addEventListener("click", () => {
        const value = minutesInput.value.trim().replace(",", ".");
        const minutes = parseFloat(value);

        if (isNaN(minutes) || minutes <= 0) {
            alert("Prosim vpiši pozitivno število minut.");
            return;
        }

        totalSeconds = Math.round(minutes * 60);
        seconds = 0;
        timerDisplay.textContent = formatTime(seconds);
        updateCircle();
        hideModal();
        beginCountdown();
    });

    // Modal – Cancel
    modalCancel.addEventListener("click", () => {
        hideModal();
    });

    // Modal – preset gumbi (5 / 15 / 25)
    presetButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            const minutes = parseFloat(btn.dataset.minutes);
            if (!isNaN(minutes) && minutes > 0) {
                minutesInput.value = minutes;
            }
        });
    });

    // Enter v inputu = confirm
    minutesInput.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            modalConfirm.click();
        }
    });
});
