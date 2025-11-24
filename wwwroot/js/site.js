document.addEventListener("DOMContentLoaded", function () {
    // --------------------------------------------------------
    // GLOBAL ERROR MODAL (uporablja quickAddErrorModal)
    // --------------------------------------------------------
    const errorModal = document.getElementById("quickAddErrorModal");
    const errorText = document.getElementById("quickAddErrorText");
    const errorClose = document.getElementById("quickAddErrorClose");

    function showError(message) {
        if (errorModal && errorText) {
            errorText.textContent = message;
            errorModal.classList.add("is-visible");
        } else {
            // fallback, če modal ne obstaja
            alert(message);
        }
    }

    function hideError() {
        if (errorModal) {
            errorModal.classList.remove("is-visible");
        }
    }

    if (errorClose) {
        errorClose.addEventListener("click", hideError);
    }
    // --------------------------------------------------------
    // TIMER LOGIC (COUNTDOWN + MODALS)
    // --------------------------------------------------------
    const timerCard = document.querySelector(".timer-card");

    if (timerCard) {
        const timerDisplay = timerCard.querySelector(".timer-inner");
        const startBtn = timerCard.querySelector(".btn-pill.primary");
        const resetBtn = timerCard.querySelector(".btn-pill.secondary");
        const timerCircle = timerCard.querySelector(".timer-circle");

        // Modal za nastavitev časa
        const modalBackdrop = document.getElementById("timerModal");
        const minutesInput = document.getElementById("timerMinutesInput");
        const presetButtons = modalBackdrop
            ? modalBackdrop.querySelectorAll(".timer-preset-btn")
            : [];
        const modalConfirm = modalBackdrop
            ? modalBackdrop.querySelector("[data-action='confirm']")
            : null;
        const modalCancel = modalBackdrop
            ? modalBackdrop.querySelector("[data-action='cancel']")
            : null;

        // Time's up modal
        const endModalBackdrop = document.getElementById("timerEndModal");
        const endYesBtn = document.getElementById("timerEndYes");
        const endNoBtn = document.getElementById("timerEndNo");
        const endHint = document.querySelector(".timer-end-hint");


        if (
            !timerDisplay ||
            !startBtn ||
            !resetBtn ||
            !timerCircle ||
            !modalBackdrop ||
            !minutesInput ||
            !modalConfirm ||
            !modalCancel ||
            !endModalBackdrop ||
            !endYesBtn ||
            !endNoBtn ||
            !endHint
        ) {

            // nekaj manjka, ne delaj timer logike
        } else {
            let timerInterval = null;
            let remainingSeconds = 0;        // koliko časa je še ostalo
            let sessionTotalSeconds = null;  // skupni čas seje
            let running = false;

            // ---------- helperji ----------

            function formatTime(sec) {
                const m = Math.floor(sec / 60);
                const s = sec % 60;
                return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
            }

            function updateDisplay() {
                timerDisplay.textContent = formatTime(remainingSeconds);
            }

            // krog se PRAZNI (poln na začetku, prazen na koncu)
            function updateCircle() {
                if (!sessionTotalSeconds || sessionTotalSeconds <= 0) {
                    timerCircle.style.setProperty("--timer-progress", "0deg");
                    return;
                }

                const ratio = Math.min(
                    Math.max(remainingSeconds / sessionTotalSeconds, 0),
                    1
                );
                const deg = ratio * 360;
                timerCircle.style.setProperty("--timer-progress", `${deg}deg`);
            }

            // ---------- modali ----------

            function showModal() {
                modalBackdrop.classList.add("is-visible");
                setTimeout(() => {
                    minutesInput.focus();
                    minutesInput.select();
                }, 10);
            }

            function hideModal() {
                modalBackdrop.classList.remove("is-visible");
            }

            function showEndModal() {
                endModalBackdrop.classList.add("is-visible");
            }

            function hideEndModal() {
                endModalBackdrop.classList.remove("is-visible");
            }

            // ---------- timer logika (COUNTDOWN) ----------

            function beginCountdown() {
                if (!sessionTotalSeconds || sessionTotalSeconds <= 0) return;

                running = true;
                startBtn.textContent = "Pause";

                clearInterval(timerInterval);
                timerInterval = setInterval(() => {
                    remainingSeconds--;

                    if (remainingSeconds <= 0) {
                        remainingSeconds = 0;
                        updateDisplay();
                        updateCircle();

                        clearInterval(timerInterval);
                        running = false;
                        startBtn.textContent = "Start";

                        showEndModal();
                        return;
                    }

                    updateDisplay();
                    updateCircle();
                }, 1000);
            }

            // Start / Pause
            startBtn.addEventListener("click", () => {
                if (!running) {
                    if (!sessionTotalSeconds) {
                        // prva seja: še nimamo časa
                        showModal();
                    } else {
                        // imamo čas
                        if (remainingSeconds <= 0) {
                            // prejšnja seja je končana -> začni znova
                            remainingSeconds = sessionTotalSeconds;
                            updateDisplay();
                            updateCircle();
                        }
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

                remainingSeconds = 0;
                sessionTotalSeconds = null;
                startBtn.textContent = "Start";
                timerCircle.style.setProperty("--timer-progress", "0deg");
                timerDisplay.textContent = "00:00";
            });

            // Modal za nastavitev časa – Confirm
            modalConfirm.addEventListener("click", () => {
                const value = minutesInput.value.trim().replace(",", ".");
                const minutes = parseFloat(value);

                if (isNaN(minutes) || minutes <= 0) {
                    showError("Please enter a valid duration in minutes.");
                    return;
                }

                sessionTotalSeconds = Math.round(minutes * 60);
                remainingSeconds = sessionTotalSeconds;

                updateDisplay();
                updateCircle();
                hideModal();
                beginCountdown();
            });

            // Cancel setup modal
            modalCancel.addEventListener("click", () => {
                hideModal();
            });

            // Preset gumbi (5/15/25)
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

            // ---------- "Time's up" modal logika ----------

            // Yes -> samo zapre modal
            endYesBtn.addEventListener("click", () => {
                hideEndModal();
            });

            // No -> 50% prejšnjega časa, countdown od začetka
            endNoBtn.addEventListener("click", () => {
                hideEndModal();

                if (!sessionTotalSeconds || sessionTotalSeconds <= 0) return;

                sessionTotalSeconds = Math.max(1, Math.round(sessionTotalSeconds * 0.5));
                remainingSeconds = sessionTotalSeconds;

                updateDisplay();
                updateCircle();
                beginCountdown();
            });
            // hover na "No :(" pokaže hint pod obema gumboma
            endNoBtn.addEventListener("mouseenter", () => {
                endHint.classList.add("is-visible");
            });

            endNoBtn.addEventListener("mouseleave", () => {
                endHint.classList.remove("is-visible");
        });
        }
    }

    // --------------------------------------------------------
    // TASKI: fade-out na checkbox
    // --------------------------------------------------------
    function bindTaskItem(li) {
        const checkbox = li.querySelector(".task-checkbox-input");
        if (!checkbox) return;

        checkbox.addEventListener("change", () => {
            if (checkbox.checked) {
                li.classList.add("completed");
                setTimeout(() => {
                    li.remove();
                }, 300); // malo več kot CSS transition (0.25s)
            }
        });
    }

    document.querySelectorAll(".task-item").forEach(bindTaskItem);

    // --------------------------------------------------------
    // QUICK ADD LOGIC (vključno z error popupom)
    // --------------------------------------------------------
    const quickAddCard = document.querySelector(".quick-add-card");
    const taskList = document.querySelector(".task-list");
    const quickAddErrorModal = document.getElementById("quickAddErrorModal");
    const quickAddErrorClose = document.getElementById("quickAddErrorClose");
    const quickAddErrorText = document.getElementById("quickAddErrorText");


    if (quickAddCard && taskList) {
        const quickInput = quickAddCard.querySelector(".quick-input");
        const quickAddBtn = quickAddCard.querySelector(".btn-add");

        const quickAddModal = document.getElementById("quickAddModal");
        const quickAddTaskLabel = document.getElementById("quickAddTaskLabel");
        const quickAddMinutesInput = document.getElementById("quickAddMinutesInput");
        const quickAddSubjectSelect = document.getElementById("quickAddSubject");
        const quickAddConfirm = document.getElementById("quickAddConfirm");
        const quickAddCancel = document.getElementById("quickAddCancel");
        const quickPresets = quickAddModal
            ? quickAddModal.querySelectorAll(".quickadd-preset")
            : [];

        const quickAddErrorModal = document.getElementById("quickAddErrorModal");
        const quickAddErrorClose = document.getElementById("quickAddErrorClose");

        if (
            quickInput &&
            quickAddBtn &&
            quickAddModal &&
            quickAddTaskLabel &&
            quickAddMinutesInput &&
            quickAddSubjectSelect &&
            quickAddConfirm &&
            quickAddCancel &&
            quickAddErrorModal &&
            quickAddErrorClose &&
            quickAddErrorText
        ) {

            function showQuickAddModal(taskName) {
                quickAddTaskLabel.textContent = taskName;
                quickAddMinutesInput.value = 25;
                quickAddSubjectSelect.value = "";
                quickAddModal.classList.add("is-visible");

                setTimeout(() => {
                    quickAddMinutesInput.focus();
                    quickAddMinutesInput.select();
                }, 10);
            }

            function hideQuickAddModal() {
                quickAddModal.classList.remove("is-visible");
            }

            function openModalFromInput() {
                const taskName = quickInput.value.trim();
                if (!taskName) {
                    showQuickAddError("You need to add a task description first...");
                    return;
                }
                showQuickAddModal(taskName);
            }


            // klik na gumb Add
            quickAddBtn.addEventListener("click", () => {
                openModalFromInput();
            });

            // Enter v input polju = Add
            quickInput.addEventListener("keydown", (e) => {
                if (e.key === "Enter") {
                    e.preventDefault();
                    openModalFromInput();
                }
            });

            // preset gumbi (5/10/25)
            quickPresets.forEach(btn => {
                btn.addEventListener("click", () => {
                    const minutes = parseFloat(btn.dataset.minutes);
                    if (!isNaN(minutes) && minutes > 0) {
                        quickAddMinutesInput.value = minutes;
                    }
                });
            });

            // Cancel
            quickAddCancel.addEventListener("click", () => {
                hideQuickAddModal();
            });

            // Confirm – doda task v Today’s Tasks
            quickAddConfirm.addEventListener("click", () => {
                const rawTaskName = quickInput.value.trim();
                if (!rawTaskName) {
                    showError("You need to add a task description first...");
                    return;
                }



                const minutesStr = quickAddMinutesInput.value.trim().replace(",", ".");
                const minutes = parseFloat(minutesStr);
                if (isNaN(minutes) || minutes <= 0) {
                    showError("Please enter a valid duration in minutes.");
                    return;
                }



                const subject = quickAddSubjectSelect.value;

                const li = document.createElement("li");
                li.className = "task-item";

                const left = document.createElement("label");
                left.className = "task-left";

                const checkboxInput = document.createElement("input");
                checkboxInput.type = "checkbox";
                checkboxInput.className = "task-checkbox-input";

                const checkboxVisual = document.createElement("span");
                checkboxVisual.className = "task-checkbox";

                const textWrapper = document.createElement("div");
                const titleSpan = document.createElement("span");
                titleSpan.textContent = rawTaskName;
                textWrapper.appendChild(titleSpan);

                if (subject) {
                    const subjectLabel = document.createElement("span");
                    subjectLabel.className = "task-subject";
                    subjectLabel.textContent = subject;
                    textWrapper.appendChild(subjectLabel);
                }

                left.appendChild(checkboxInput);
                left.appendChild(checkboxVisual);
                left.appendChild(textWrapper);

                const durationSpan = document.createElement("span");
                durationSpan.className = "task-duration";
                durationSpan.textContent = `${Math.round(minutes)}m`;

                li.appendChild(left);
                li.appendChild(durationSpan);

                taskList.appendChild(li);

                // fade-out logika na nov task
                bindTaskItem(li);

                quickInput.value = "";
                hideQuickAddModal();
            });
        }
    }

    // Add Subject
    const showBtn = document.getElementById("showInputBtn");
    const form = document.getElementById("addSubjectForm");

    if (showBtn && form) {
        showBtn.addEventListener("click", function () {
            form.classList.toggle('d-none');
            const input = form.querySelector('input[name="subjectName"]');
            if (input) input.focus();
        });
    }
});
