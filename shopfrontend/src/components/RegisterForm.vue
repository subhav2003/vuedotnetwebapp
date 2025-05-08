<template>
  <div class="register-container flex justify-center items-start pt-8 px-4 w-full min-h-[85vh] bg-[#0f0f0f] text-[#C8B280]">
    <Toast :toasts="toasts" @close="removeToast" />

    <form
        @submit.prevent="register"
        class="bg-[#121212] border border-[#2a2a2a] p-6 rounded-lg shadow-xl w-full max-w-lg animate-slide-fade"
    >
      <h2 class="text-3xl font-bold text-center mb-6">Register</h2>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
        <div>
          <label for="name" class="block mb-2">Name:</label>
          <input
              type="text" id="name"
              v-model="form.name"
              placeholder="Enter your name"
              class="input-field"
              required
          />
        </div>
        <div>
          <label for="email" class="block mb-2">Email:</label>
          <input
              type="email" id="email"
              v-model="form.email"
              placeholder="Enter your email"
              class="input-field"
              required
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
        <div>
          <label for="password" class="block mb-2">Password:</label>
          <input
              type="password" id="password"
              v-model="form.password"
              placeholder="Enter your password"
              class="input-field"
              required
          />
        </div>
        <div>
          <label for="password_confirmation" class="block mb-2">Confirm Password:</label>
          <input
              type="password" id="password_confirmation"
              v-model="form.password_confirmation"
              placeholder="Confirm your password"
              class="input-field"
              required
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
        <div>
          <label for="phone" class="block mb-2">Phone:</label>
          <input
              type="text" id="phone"
              v-model="form.phone"
              placeholder="Enter your phone number"
              class="input-field"
          />
        </div>
        <div>
          <label for="dateOfBirth" class="block mb-2">Date of Birth:</label>
          <input
              type="date" id="dateOfBirth"
              v-model="form.dateOfBirth"
              class="input-field"
          />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
        <div>
          <label for="gender" class="block mb-2">Gender:</label>
          <select id="gender" v-model="form.gender" class="input-field">
            <option value="" disabled>Select your gender</option>
            <option value="male">Male</option>
            <option value="female">Female</option>
            <option value="other">Other</option>
          </select>
        </div>
        <div>
          <label for="username" class="block mb-2">Username:</label>
          <input
              type="text" id="username"
              v-model="form.username"
              placeholder="Choose a username"
              class="input-field"
          />
        </div>
      </div>

      <button
          type="submit"
          :disabled="isSubmitting"
          class="w-full bg-[#A47148] hover:bg-[#FFD700] text-black py-2 rounded-lg transition duration-300 flex justify-center items-center"
      >
        <span v-if="isSubmitting" class="loader w-4 h-4 mr-2"></span>
        <span v-else>Register</span>
      </button>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import axios from 'axios';
import { useRouter } from 'vue-router';
import Toast from '@/components/Toast.vue';

const apiBaseUrl = process.env.VUE_APP_API_URL;
const router = useRouter();

const form = ref({
  name: '',
  email: '',
  password: '',
  password_confirmation: '',
  phone: '',
  dateOfBirth: '',
  gender: '',
  username: ''
});

const isSubmitting = ref(false);
const toasts = reactive([]);

function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
}

function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}

const register = async () => {
  if (form.value.password !== form.value.password_confirmation) {
    showToast("Passwords do not match", "error");
    return;
  }

  isSubmitting.value = true;

  try {
    const payload = {
      ...form.value,
      role: 'member'
    };

    delete payload.password_confirmation;

    const { data } = await axios.post(`${apiBaseUrl}/api/account/signup`, payload);

    showToast('Registration successful!', 'success');
    router.push({ name: 'home' });
  } catch (error) {
    const errs = error.response?.data?.errors;
    if (errs) {
      Object.values(errs).flat().forEach(msg => showToast(msg, 'error'));
    } else {
      const msg = error.response?.data?.message || error.response?.data?.error || 'Registration failed.';
      showToast(msg, 'error');
    }
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<style scoped>
.input-field {
  @apply w-full p-2 bg-[#181818] text-[#C8B280] border border-[#3a3a3a] rounded-lg focus:outline-none focus:ring-2 focus:ring-[#C8B280];
}

.loader {
  border: 3px solid #444;
  border-top: 3px solid #FFD700;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.animate-slide-fade {
  animation: slideFade 0.4s ease-in-out both;
}
@keyframes slideFade {
  0% {
    opacity: 0;
    transform: translateY(10px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
