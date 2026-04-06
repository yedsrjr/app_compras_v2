// Validação de senha em tempo real
document.getElementById('Password')?.addEventListener('input', function (e) {
    const password = e.target.value;
    const hint = document.querySelector('.password-hint');

    const hasMinLength = password.length >= 6;

    if (hasMinLength) {
        hint.style.color = '#28a745';
        hint.textContent = '✓ Senha válida';
    } else {
        hint.style.color = '#6c757d';
        hint.textContent = 'Mínimo 6 caracteres';
    }
});

// Validação de confirmação de senha
document.getElementById('ConfirmPassword')?.addEventListener('input', function (e) {
    const password = document.getElementById('Password').value;
    const confirmPassword = e.target.value;

    const container = e.target.parentNode;
    // Procura pelo span de validação do ASP.NET/jQuery (data-valmsg-for)
    let validationSpan = container.querySelector('span[data-valmsg-for="ConfirmPassword"]');
    if (!validationSpan) {
        validationSpan = container.querySelector('.validation-message, .field-validation-error, .field-validation-valid');
    }

    // Remove mensagens duplicadas criadas anteriormente
    container.querySelectorAll('.validation-js').forEach(span => span.remove());

    if (confirmPassword && password !== confirmPassword) {
        if (!validationSpan) {
            const span = document.createElement('span');
            span.className = 'validation-message validation-js';
            span.setAttribute('data-valmsg-for', 'ConfirmPassword');
            span.textContent = 'As senhas não coincidem';
            container.appendChild(span);
            return;
        }
        // Usa o span existente do ASP.NET/jQuery
        validationSpan.textContent = 'As senhas não coincidem';
        validationSpan.style.display = 'block';
    } else {
        if (validationSpan) {
            validationSpan.textContent = '';
            validationSpan.style.display = 'none';
        }
    }
});
