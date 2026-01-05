// ==========================
// StudyBuddy persistent timer (GLOBAL)
// ==========================
(function () {
  const STORAGE_KEY = "sb_timer_v1";

  const ORIGINAL_TITLE = document.title;
  let tickInterval = null;

  function readState() {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  function writeState(state) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  }

  function clearState() {
    localStorage.removeItem(STORAGE_KEY);
  }

  function pad2(n) {
    return String(n).padStart(2, "0");
  }

  function formatMMSS(totalSeconds) {
    const m = Math.floor(totalSeconds / 60);
    const s = totalSeconds % 60;
    return `${pad2(m)}:${pad2(s)}`;
  }

  function setTabTitle(secondsLeft) {
    if (secondsLeft > 0) {
      document.title = `⏳ ${formatMMSS(secondsLeft)} • ${ORIGINAL_TITLE}`;
    } else {
      document.title = ORIGINAL_TITLE;
    }
  }

  function secondsLeftFromState(state) {
    if (!state || !state.running) return 0;

    const now = Date.now();
    if (state.endTimeMs) {
      return Math.max(0, Math.ceil((state.endTimeMs - now) / 1000));
    }

    // fallback if old format
    if (typeof state.remainingSeconds === "number") return Math.max(0, state.remainingSeconds);
    return 0;
  }

  function updateTimerUIFromState() {
    const state = readState();
    const secLeft = secondsLeftFromState(state);

    // Update title always
    setTabTitle(secLeft);

    // Update timer UI only if exists on this page
    const timerInner = document.querySelector(".timer-inner");
    if (timerInner) timerInner.textContent = formatMMSS(secLeft);

    // Subject label
    const label = document.getElementById("timerSubjectLabel");
    if (label) {
      const subj = state?.subjectName?.trim();
      if (subj) {
        label.textContent = subj;
        label.classList.add("is-visible");
      } else {
        label.textContent = "";
        label.classList.remove("is-visible");
      }
    }

    return secLeft;
  }

  function stopTicking() {
    if (tickInterval) {
      clearInterval(tickInterval);
      tickInterval = null;
    }
    setTabTitle(0);
  }

  function startTicking() {
    stopTicking();

    tickInterval = setInterval(() => {
      const state = readState();
      if (!state || !state.running) {
        stopTicking();
        updateTimerUIFromState();
        return;
      }

      const secLeft = updateTimerUIFromState();

      if (secLeft <= 0) {
        // finished
        state.running = false;
        state.finished = true;
        state.paused = false;
        state.remainingMs = 0;
        writeState(state);

        stopTicking();
        updateTimerUIFromState();

        // Show end modal if exists
        const endModal = document.getElementById("timerEndModal");
        if (endModal) endModal.classList.add("is-visible");
      }
    }, 250);
  }

  function startNew(durationMinutes, subjectName) {
    const durationMs = Math.max(1, durationMinutes) * 60 * 1000;
    const now = Date.now();

    const state = {
      running: true,
      paused: false,
      finished: false,
      startedAtMs: now,
      endTimeMs: now + durationMs,
      remainingMs: durationMs,
      durationMinutes: durationMinutes,
      subjectName: subjectName || ""
    };

    writeState(state);
    startTicking();
    updateTimerUIFromState();
  }

  function pause() {
    const state = readState();
    if (!state || !state.running) return;

    const now = Date.now();
    const remainingMs = Math.max(0, (state.endTimeMs || now) - now);

    state.running = false;
    state.paused = true;
    state.finished = false;
    state.remainingMs = remainingMs;
    state.endTimeMs = null;

    writeState(state);
    stopTicking();
    updateTimerUIFromState();
  }

  function resume() {
    const state = readState();
    if (!state || !state.paused) return;

    const now = Date.now();
    const remainingMs = Math.max(0, state.remainingMs || 0);

    state.running = true;
    state.paused = false;
    state.finished = false;
    state.endTimeMs = now + remainingMs;

    writeState(state);
    startTicking();
    updateTimerUIFromState();
  }

  function reset() {
    clearState();
    stopTicking();
    updateTimerUIFromState();
  }

  // expose
  window.StudyBuddyTimer = {
    startNew,
    pause,
    resume,
    reset,
    readState,
    writeState,
    updateTimerUIFromState,
    startTicking
  };

  // auto resume ticking if timer is already running
  const initial = readState();
  if (initial?.running) startTicking();
  updateTimerUIFromState();

  // sync between tabs
  window.addEventListener("storage", (e) => {
    if (e.key === STORAGE_KEY) {
      const st = readState();
      if (st?.running) startTicking();
      else stopTicking();
      updateTimerUIFromState();
    }
  });
})();


// ==========================
// MAIN APP INIT (runs once)
// ==========================
if (window.studyBuddyInitialized) {
  // already initialized
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

    if (errorClose) errorClose.addEventListener("click", hideError);

    // --------------------------------------------------------
    // TIMER LOGIC (UI + MODALS) - uses persistent timer above
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

      const presetButtons = modalBackdrop ? modalBackdrop.querySelectorAll(".timer-preset-btn") : [];
      const modalConfirm = modalBackdrop ? modalBackdrop.querySelector("[data-action='confirm']") : null;
      const modalCancel = modalBackdrop ? modalBackdrop.querySelector("[data-action='cancel']") : null;

      const createStudySessionUrl = window.studyBuddyConfig?.createStudySessionUrl || null;

      if (
        !timerDisplay || !startBtn || !resetBtn || !timerCircle ||
        !modalBackdrop || !minutesInput || !modalConfirm || !modalCancel ||
        !endModalBackdrop || !endYesBtn || !endNoBtn || !endHint
      ) {
        // missing elements -> do nothing
      } else {

        function formatTime(sec) {
          const m = Math.floor(sec / 60);
          const s = sec % 60;
          return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`;
        }

        function getState() {
          return window.StudyBuddyTimer.readState();
        }

        function getSecondsLeft() {
          const st = getState();
          if (!st) return 0;
          if (st.running && st.endTimeMs) return Math.max(0, Math.ceil((st.endTimeMs - Date.now()) / 1000));
          if (st.paused && typeof st.remainingMs === "number") return Math.max(0, Math.ceil(st.remainingMs / 1000));
          return 0;
        }

        function updateDisplayFromState() {
          const secLeft = getSecondsLeft();
          timerDisplay.textContent = formatTime(secLeft);

          // progress circle (based on total duration)
          const st = getState();
          const totalMs = (st?.durationMinutes ? st.durationMinutes * 60 * 1000 : 0);
          const leftMs = secLeft * 1000;

          if (!totalMs || totalMs <= 0) {
            timerCircle.style.setProperty("--timer-progress", "0deg");
          } else {
            const ratio = Math.min(Math.max(leftMs / totalMs, 0), 1);
            const deg = ratio * 360;
            timerCircle.style.setProperty("--timer-progress", `${deg}deg`);
          }

          // subject label
          const subj = st?.subjectName?.trim() || "";
          if (subjectLabel) {
            if (subj) {
              subjectLabel.textContent = subj;
              subjectLabel.classList.add("is-visible");
            } else {
              subjectLabel.textContent = "";
              subjectLabel.classList.remove("is-visible");
            }
          }

          // button text
          if (st?.running) startBtn.textContent = "Pause";
          else if (st?.paused) startBtn.textContent = "Resume";
          else startBtn.textContent = "Start";
        }

        function showModal() {
          modalBackdrop.classList.add("is-visible");

          const st = getState();
          if (subjectSelect) subjectSelect.value = st?.subjectName || "";

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

        function saveStudySession(durationMinutes, subjectName) {
          if (!createStudySessionUrl) return;
          const minutesInt = Math.max(1, Math.round(durationMinutes));

          fetch(createStudySessionUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json", "Accept": "application/json" },
            body: JSON.stringify({
              durationMinutes: minutesInt,
              subjectName: subjectName || null
            })
          }).catch(err => console.error("Failed to save study session", err));
        }

        // On page load: reflect current persistent state
        updateDisplayFromState();

        // Also keep UI in sync while ticking (global ticker updates .timer-inner already,
        // but circle + button text are handled here)
        setInterval(() => {
          if (timerCard) updateDisplayFromState();
        }, 500);

        // Start/Pause/Resume button
        startBtn.addEventListener("click", () => {
          const st = getState();

          if (!st || (!st.running && !st.paused && !st.finished && !st.durationMinutes)) {
            // no timer configured -> open modal
            showModal();
            return;
          }

          if (st.running) {
            window.StudyBuddyTimer.pause();
            updateDisplayFromState();
            return;
          }

          if (st.paused) {
            window.StudyBuddyTimer.resume();
            updateDisplayFromState();
            return;
          }

          // finished or idle with known duration -> start fresh using stored duration
          const mins = st.durationMinutes || 25;
          const subj = st.subjectName || "";
          window.StudyBuddyTimer.startNew(mins, subj);
          updateDisplayFromState();
        });

        // Reset
        resetBtn.addEventListener("click", () => {
          const st = getState();
          const totalMinutes = st?.durationMinutes || 0;
          const leftSeconds = getSecondsLeft();
          const elapsedMinutes = Math.max(0, totalMinutes - (leftSeconds / 60));

          if (elapsedMinutes > 0) {
            saveStudySession(elapsedMinutes, st?.subjectName || "");
          }

          window.StudyBuddyTimer.reset();

          // also clear subject dropdown/label UI
          if (subjectLabel) {
            subjectLabel.textContent = "";
            subjectLabel.classList.remove("is-visible");
          }
          if (subjectSelect) subjectSelect.value = "";

          updateDisplayFromState();
        });

        // Modal Confirm
        modalConfirm.addEventListener("click", () => {
          const value = minutesInput.value.trim().replace(",", ".");
          const minutes = parseFloat(value);

          if (isNaN(minutes) || minutes <= 0) {
            showError("Please enter a valid duration in minutes.");
            return;
          }

          const subjectName = subjectSelect ? subjectSelect.value.trim() : "";

          // Start persistent timer
          window.StudyBuddyTimer.startNew(minutes, subjectName);

          hideModal();
          updateDisplayFromState();
        });

        modalCancel.addEventListener("click", hideModal);

        // presets
        presetButtons.forEach(btn => {
          btn.addEventListener("click", () => {
            const minutes = parseFloat(btn.dataset.minutes);
            if (!isNaN(minutes) && minutes > 0) minutesInput.value = minutes;
          });
        });

        minutesInput.addEventListener("keydown", (e) => {
          if (e.key === "Enter") {
            e.preventDefault();
            modalConfirm.click();
          }
        });

        // End modal YES (save full session, clear timer)
        endYesBtn.addEventListener("click", () => {
          const st = getState();
          const fullMinutes = st?.durationMinutes || 0;

          if (fullMinutes > 0) {
            saveStudySession(fullMinutes, st?.subjectName || "");
          }

          hideEndModal();
          window.StudyBuddyTimer.reset();
          updateDisplayFromState();
        });


        // End modal NO (save full, start new 50%)
        endNoBtn.addEventListener("click", () => {
          const st = getState();
          const fullMinutes = st?.durationMinutes || 0;
          const subj = st?.subjectName || "";

          if (fullMinutes > 0) {
            saveStudySession(fullMinutes, subj);
          }

          hideEndModal();

          const halfMinutes = Math.max(1, Math.round(fullMinutes * 0.5));
          window.StudyBuddyTimer.startNew(halfMinutes, subj);

          updateDisplayFromState();
        });
        endNoBtn.addEventListener("mouseenter", () => endHint.classList.add("is-visible"));
        endNoBtn.addEventListener("mouseleave", () => endHint.classList.remove("is-visible"));
      }
    }

    // --------------------------------------------------------
    // TASKS: fade-out on checkbox
    // --------------------------------------------------------
    function bindTaskItem(li) {
      const checkbox = li.querySelector(".task-checkbox-input");
      if (!checkbox) return;

      checkbox.addEventListener("change", () => {
        if (checkbox.checked) {
          li.classList.add("completed");
          setTimeout(() => li.remove(), 300);
        }
      });
    }

    document.querySelectorAll(".task-item").forEach(bindTaskItem);

    // --------------------------------------------------------
    // QUICK ADD LOGIC (unchanged)
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
      const quickPresets = quickAddModal ? quickAddModal.querySelectorAll(".quickadd-preset") : [];

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

        quickAddBtn.addEventListener("click", () => {
          const taskName = quickInput.value.trim();
          if (!taskName) {
            showError("You need to add a task description first...");
            return;
          }
          showQuickAddModal(taskName);
        });

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

        quickPresets.forEach(btn => {
          btn.addEventListener("click", () => {
            const minutes = parseFloat(btn.dataset.minutes);
            if (!isNaN(minutes) && minutes > 0) quickAddMinutesInput.value = minutes;
          });
        });

        quickAddCancel.addEventListener("click", hideQuickAddModal);

        quickAddConfirm.addEventListener("click", async () => {
          const rawTaskName = (currentTaskNameForModal || "").trim();
          if (!rawTaskName) {
            hideQuickAddModal();
            return;
          }

          const minutesStr = quickAddMinutesInput.value.trim().replace(",", ".");
          const minutes = parseFloat(minutesStr);
          if (isNaN(minutes) || minutes <= 0) {
            showError("Prosimo vnesite veljaven čas v minutah.");
            return;
          }

          const subject = quickAddSubjectSelect.value;
          const createUrl = window.studyBuddyConfig?.createQuickTaskUrl || null;

          if (!createUrl) {
            appendQuickTaskToDom(rawTaskName, subject, minutes);
            quickInput.value = "";
            currentTaskNameForModal = "";
            hideQuickAddModal();
            return;
          }

          quickAddConfirm.disabled = true;
          try {
            const res = await fetch(createUrl, {
              method: "POST",
              credentials: "same-origin",
              headers: { "Content-Type": "application/json", "Accept": "application/json" },
              body: JSON.stringify({ description: rawTaskName, subjectName: subject })
            });

            if (!res.ok) {
              if (res.status === 401) { showError("Prosimo se prijavite, da lahko dodajate naloge."); return; }
              if (res.status === 403) { showError("Nimate dovoljenja za to dejanje."); return; }

              let json = null;
              try { json = await res.json(); } catch (e) { }
              const err = json?.error || null;
              if (err === "subject_missing") showError("Prosimo izberite predmet.");
              else if (err === "subject_not_found") showError("Izbrani predmet ne obstaja.");
              else if (err === "empty_description") showError("Dodajte opis naloge.");
              else showError("Napaka pri shranjevanju naloge.");
              return;
            }

            const data = await res.json();
            const task = data?.task;
            const desc = task?.description || rawTaskName;
            const subj = task?.subjectName || subject;
            appendQuickTaskToDom(desc, subj, minutes);

            quickInput.value = "";
            currentTaskNameForModal = "";
            hideQuickAddModal();
          } catch (e) {
            console.error("Quick add network error:", e);
            showError("Network error while saving task. Check console for details.");
          } finally {
            quickAddConfirm.disabled = false;
          }
        });

        function appendQuickTaskToDom(title, subject, minutes) {
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
          titleSpan.textContent = title;
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
        }
      }
    }

    // --------------------------------------------------------
    // ADD SUBJECT MODAL (unchanged)
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
