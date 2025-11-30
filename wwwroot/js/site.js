// Prevent double initialization if site.js is included more than once
if (window.studyBuddyInitialized) {
    // already initialized, do nothing
} else {
    window.studyBuddyInitialized = true;

    document.addEventListener("DOMContentLoaded", function () {

        // --------------------------------------------------------
        // GLOBAL ERROR MODAL
        // --------------------------------------------------------
        const errorModal = document.getElementById("quickAddErrorModal");
        const errorText = document.getElementById("quickAddErrorText");
        const errorClose = document.getElementById("quickAddErrorClose");

        function showError(message) {
            if (errorModal && errorText) {
                errorText.textContent = message;
                errorModal.classList.add("is-visible");
            } else {
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

            const modalBackdrop = document.getElementById("timerModal");
            const minutesInput = document.getElementById("timerMinutesInput");
            const subjectSelect = document.getElementById("timerSubjectSelect");
            const subjectLabel = document.getElementById("timerSubjectLabel");

            const endModalBackdrop = document.getElementById("timerEndModal");
            const endYesBtn = document.getElementById("timerEndYes");
            const endNoBtn = document.getElementById("timerEndNo");
            const endHint = document.querySelector(".timer-end-hint");

            const presetButtons = modalBackdrop
                ? modalBackdrop.querySelectorAll(".timer-preset-btn")
                : [];
            const modalConfirm = modalBackdrop
                ? modalBackdrop.querySelector("[data-action='confirm']")
                : null;
            const modalCancel = modalBackdrop
                ? modalBackdrop.querySelector("[data-action='cancel']")
                : null;

            // URL za zapis study sessiona
            const createStudySessionUrl = window.studyBuddyConfig?.createStudySessionUrl || null;

            let timerInterval = null;
            let remainingSeconds = 0;
            let sessionTotalSeconds = 0;
            let running = false;
            let currentSubjectName = "";

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
                // nekaj manjka, timerja ne inicializiramo
            } else {
                // ---------- helperji ----------

                function formatTime(sec) {
                    const m = Math.floor(sec / 60);
                    const s = sec % 60;
                    return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
                }

                function updateDisplay() {
                    timerDisplay.textContent = formatTime(remainingSeconds);
                }

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

                function saveStudySession(durationMinutes) {
                    if (!createStudySessionUrl) return;

                    const minutesInt = Math.max(1, Math.round(durationMinutes));

                    fetch(createStudySessionUrl, {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json",
                            "Accept": "application/json"
                        },
                        body: JSON.stringify({
                            durationMinutes: minutesInt,
                            subjectName: currentSubjectName || null
                        })
                    }).catch(err => {
                        console.error("Failed to save study session", err);
                    });
                }

                function getElapsedMinutes() {
                    if (!sessionTotalSeconds || sessionTotalSeconds <= 0) return 0;
                    const elapsedSeconds = sessionTotalSeconds - remainingSeconds;
                    return Math.max(0, elapsedSeconds / 60);
                }

                function resetAllTimerState(clearSubject) {
                    running = false;
                    clearInterval(timerInterval);

                    remainingSeconds = 0;
                    sessionTotalSeconds = 0;
                    startBtn.textContent = "Start";
                    timerCircle.style.setProperty("--timer-progress", "0deg");
                    timerDisplay.textContent = "00:00";

                    if (clearSubject) {
                        currentSubjectName = "";
                        if (subjectLabel) {
                            subjectLabel.textContent = "";
                            subjectLabel.classList.remove("is-visible");
                        }
                        if (subjectSelect) {
                            subjectSelect.value = "";
                        }
                    }
                }

                // ---------- modali ----------

                function showModal() {
                    modalBackdrop.classList.add("is-visible");

                    if (subjectSelect) {
                        subjectSelect.value = currentSubjectName || "";
                    }

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
                            // prvič nastavljamo čas
                            showModal();
                        } else {
                            // že imamo čas
                            if (remainingSeconds <= 0) {
                                remainingSeconds = sessionTotalSeconds;
                                updateDisplay();
                                updateCircle();
                            }
                            beginCountdown();
                        }
                    } else {
                        running = false;
                        startBtn.textContent = "Start";
                        clearInterval(timerInterval);
                    }
                });

                // Reset (zapišemo delno sejo, če je čas tekel)
                resetBtn.addEventListener("click", () => {
                    const elapsedMinutes = getElapsedMinutes();
                    if (elapsedMinutes > 0) {
                        saveStudySession(elapsedMinutes);
                    }

                    resetAllTimerState(true);
                });

                // Modal Confirm – nastavi novo sejo
                modalConfirm.addEventListener("click", () => {
                    const value = minutesInput.value.trim().replace(",", ".");
                    const minutes = parseFloat(value);

                    if (isNaN(minutes) || minutes <= 0) {
                        showError("Please enter a valid duration in minutes.");
                        return;
                    }

                    currentSubjectName = subjectSelect ? subjectSelect.value.trim() : "";

                    if (subjectLabel) {
                        if (currentSubjectName) {
                            subjectLabel.textContent = currentSubjectName;
                            subjectLabel.classList.add("is-visible");
                        } else {
                            subjectLabel.textContent = "";
                            subjectLabel.classList.remove("is-visible");
                        }
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

                // Yes -> zapišemo celotno sejo in resetiramo
                endYesBtn.addEventListener("click", () => {
                    if (sessionTotalSeconds && sessionTotalSeconds > 0) {
                        const fullMinutes = sessionTotalSeconds / 60;
                        saveStudySession(fullMinutes);
                    }

                    hideEndModal();
                    resetAllTimerState(false);
                });

                // No -> zapišemo sejo + začnemo novo 50% sejo
                endNoBtn.addEventListener("click", () => {
                    let fullMinutes = 0;
                    if (sessionTotalSeconds && sessionTotalSeconds > 0) {
                        fullMinutes = sessionTotalSeconds / 60;
                        saveStudySession(fullMinutes);
                    }

                    hideEndModal();

                    if (!sessionTotalSeconds || sessionTotalSeconds <= 0) return;

                    sessionTotalSeconds = Math.max(1, Math.round(sessionTotalSeconds * 0.5));
                    remainingSeconds = sessionTotalSeconds;

                    updateDisplay();
                    updateCircle();
                    beginCountdown();
                });

                // hover na "No" pokaže hint
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
                    }, 300);
                }
            });
        }

        document.querySelectorAll(".task-item").forEach(bindTaskItem);

        // --------------------------------------------------------
        // QUICK ADD LOGIC
        // --------------------------------------------------------
        const quickAddCard = document.querySelector(".quick-add-card");
        const taskList = document.querySelector(".task-list");

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

            // 🔹 opis taska, ki ga hranimo med modalom
            let currentTaskNameForModal = "";

            if (
                quickInput &&
                quickAddBtn &&
                quickAddModal &&
                quickAddTaskLabel &&
                quickAddMinutesInput &&
                quickAddSubjectSelect &&
                quickAddConfirm &&
                quickAddCancel
            ) {

                function showQuickAddModal(taskName) {
                    currentTaskNameForModal = taskName;
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

                // klik na gumb Add
                quickAddBtn.addEventListener("click", () => {
                    const taskName = quickInput.value.trim();
                    if (!taskName) {
                        // edini kraj, kjer pokažemo ta popup
                        showError("You need to add a task description first...");
                        return;
                    }
                    showQuickAddModal(taskName);
                });

                // Enter v input polju = Add
                quickInput.addEventListener("keydown", (e) => {
                    if (e.key === "Enter") {
                        e.preventDefault();
                        const taskName = quickInput.value.trim();
                        if (!taskName) {
                            showError("You need to add a task description first...");
                            return;
                        }
                        showQuickAddModal(taskName);
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
                    const rawTaskName = (currentTaskNameForModal || "").trim();

                    // Če se je slučajno izgubilo ime, samo zapremo modal brez errorja
                    if (!rawTaskName) {
                        hideQuickAddModal();
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

                    bindTaskItem(li);

                    // reset
                    quickInput.value = "";
                    currentTaskNameForModal = "";
                    hideQuickAddModal();
                });
            }
        }

        // --------------------------------------------------------
        // ADD SUBJECT MODAL
        // --------------------------------------------------------
        const showBtn = document.getElementById("showInputBtn");

        const addSubjectModal = document.getElementById("addSubjectModal");
        const addSubjectClose = document.getElementById("addSubjectClose");
        const addSubjectCancel = document.getElementById("addSubjectCancel");
        const addSubjectInput = document.getElementById("addSubjectInput");
        const addSubjectModalForm = document.getElementById("addSubjectModalForm");

        function showAddSubjectModal() {
            if (!addSubjectModal) return;
            addSubjectModal.classList.add("is-visible");

            setTimeout(() => {
                if (addSubjectInput) {
                    addSubjectInput.focus();
                    addSubjectInput.select();
                }
            }, 10);
        }

        function hideAddSubjectModal() {
            if (!addSubjectModal) return;
            addSubjectModal.classList.remove("is-visible");
        }

        if (showBtn) {
            showBtn.addEventListener("click", (e) => {
                e.preventDefault();
                showAddSubjectModal();
            });
        }

        if (addSubjectClose) addSubjectClose.addEventListener("click", hideAddSubjectModal);
        if (addSubjectCancel) addSubjectCancel.addEventListener("click", hideAddSubjectModal);

        if (addSubjectModal) {
            addSubjectModal.addEventListener("click", (e) => {
                if (e.target === addSubjectModal) hideAddSubjectModal();
            });
        }

        if (addSubjectModalForm) {
            addSubjectModalForm.addEventListener("submit", (e) => {
                const name = addSubjectInput ? addSubjectInput.value.trim() : "";
                if (!name) {
                    e.preventDefault();
                    showError("Please enter a subject name.");
                    return;
                }
                hideAddSubjectModal();
            });
        }
    });
}
