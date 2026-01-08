(function () {
  function getSelectedValue(questionEl) {
    const checked = questionEl.querySelector("input[type='radio']:checked");
    return checked ? checked.value : null;
  }

  function setLocked(questionEl, locked) {
    questionEl.querySelectorAll("input[type='radio']").forEach(r => {
      r.disabled = locked;
    });
  }

  function setState(questionEl, ok, message) {
    questionEl.classList.remove("sb-q-correct", "sb-q-wrong");
    questionEl.classList.add(ok ? "sb-q-correct" : "sb-q-wrong");

    const fb = questionEl.querySelector(".sb-quiz-feedback");
    if (fb) fb.textContent = message || "";

    // po check-u zaklenemo (da je obnašanje “preveri in konec”)
    setLocked(questionEl, true);
  }

  document.addEventListener("click", (e) => {
    const btn = e.target.closest(".sb-quiz-check");
    if (!btn) return;

    const q = btn.closest(".sb-quiz-q");
    if (!q) return;

    const selected = getSelectedValue(q);
    const correct = q.dataset.correct;

    if (!selected) {
      const fb = q.querySelector(".sb-quiz-feedback");
      if (fb) fb.textContent = "Pick an answer first.";
      return;
    }

    if (selected === correct) {
      setState(q, true, "Correct ✅");
    } else {
      setState(q, false, `Wrong ❌ Correct answer: ${correct}`);
    }
  });

  const scoreBtn = document.getElementById("sbCheckScore");
  if (scoreBtn) {
    scoreBtn.addEventListener("click", () => {
      const questions = Array.from(document.querySelectorAll(".sb-quiz-q"));

      let correctCount = 0;
      let wrongCount = 0;
      let unanswered = 0;

      questions.forEach(q => {
        const selected = getSelectedValue(q);
        const correct = q.dataset.correct;

        if (!selected) {
          unanswered++;
          return;
        }

        if (selected === correct) correctCount++;
        else wrongCount++;
      });

      const total = questions.length || 1;
      const pct = Math.round((correctCount / total) * 100);

      const banner = document.getElementById("sbScoreBanner");
      const pctEl = document.getElementById("sbScorePct");
      const detailsEl = document.getElementById("sbScoreDetails");

      if (pctEl) pctEl.textContent = `${pct}%`;
      if (detailsEl) detailsEl.textContent = `Correct: ${correctCount} • Wrong: ${wrongCount} • Unanswered: ${unanswered}`;
      if (banner) banner.style.display = "block";

      // scroll do rezultata
      if (banner) banner.scrollIntoView({ behavior: "smooth", block: "start" });
    });
  }
})();
