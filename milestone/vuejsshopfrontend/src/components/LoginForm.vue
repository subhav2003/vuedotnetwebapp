<template>
  <div class="w-full flex justify-center items-start pt-8 px-4 relative min-h-[80vh] bg-[#0f0f0f] text-[#C8B280]">
    <!-- Toast stack -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Login Form -->
    <form
        @submit.prevent="login"
        class="bg-[#121212] p-6 rounded-lg shadow-lg w-full max-w-md relative animate-fade-in border border-[#2a2a2a]"
    >
      <h2 class="text-2xl font-bold text-center mb-6">Login</h2>

      <!-- Toggle Role -->
      <div class="flex justify-between items-center mb-4 text-sm">
        <span class="text-gray-400">Logging in as:</span>
        <label class="flex items-center gap-2 cursor-pointer">
          <span :class="role === 'member' ? 'text-[#FFD700]' : 'text-gray-400'">Member</span>
          <input
              type="checkbox"
              class="hidden"
              v-model="isAdmin"
          />
          <div
              class="w-10 h-5 bg-[#333] rounded-full relative transition-colors duration-300"
              :class="{ 'bg-[#FFD700]': isAdmin }"
          >
            <div
                class="absolute top-0.5 left-0.5 w-4 h-4 rounded-full bg-white transition-transform duration-300"
                :class="{ 'translate-x-5': isAdmin }"
            />
          </div>
          <span :class="role === 'admin' ? 'text-[#FFD700]' : 'text-gray-400'">Admin</span>
        </label>
      </div>

      <!-- Email -->
      <div class="form-group mb-4">
        <label for="email" class="block mb-2">Email:</label>
        <input
            type="email"
            id="email"
            v-model="email"
            placeholder="Enter your email"
            class="w-full p-2 bg-[#181818] text-[#C8B280] border border-[#3a3a3a] rounded-lg focus:outline-none focus:ring-2 focus:ring-[#C8B280]"
            required
        />
      </div>

      <!-- Password -->
      <div class="form-group mb-2">
        <label for="password" class="block mb-2">Password:</label>
        <input
            type="password"
            id="password"
            v-model="password"
            placeholder="Enter your password"
            class="w-full p-2 bg-[#181818] text-[#C8B280] border border-[#3a3a3a] rounded-lg focus:outline-none focus:ring-2 focus:ring-[#C8B280]"
            required
        />
      </div>

      <!-- Forgot Password -->
      <div class="text-right mt-2 mb-4">
        <a
            href="#"
            class="text-sm text-[#FFD700] hover:underline"
            @click.prevent="showForgot = true"
        >
          Forgot password?
        </a>
      </div>

      <!-- Login Button -->
      <button
          type="submit"
          class="w-full bg-[#A47148] hover:bg-[#FFD700] text-black py-2 rounded-lg transition-colors duration-300 flex justify-center items-center"
          :disabled="isLoading"
      >
        <span v-if="isLoading" class="loader"></span>
        <span v-else>Login</span>
      </button>
    </form>

    <!-- Forgot Password Modal -->
    <transition name="fade">
      <div v-if="showForgot" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
        <div class="bg-[#121212] text-[#C8B280] p-6 rounded-lg shadow-lg w-full max-w-sm animate-slide-down relative border border-[#2a2a2a]">
          <button
              @click="showForgot = false"
              class="absolute top-2 right-2 text-[#C8B280] hover:text-red-500"
          >✕</button>
          <h3 class="text-lg font-bold mb-4">Reset Your Password</h3>
          <input
              v-model="forgotEmail"
              type="email"
              placeholder="Enter your email"
              class="w-full p-2 bg-[#181818] text-[#C8B280] border border-[#3a3a3a] rounded mb-4 focus:outline-none focus:ring-2 focus:ring-[#C8B280]"
          />
          <button
              @click="sendResetLink"
              :disabled="isResetting"
              class="w-full bg-[#A47148] hover:bg-[#FFD700] text-black py-2 rounded transition-colors flex justify-center items-center"
          >
            <span v-if="isResetting" class="loader w-4 h-4 mr-2"></span>
            <span v-if="!isResetting">Send Reset Link</span>
          </button>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, reactive } from 'vue';
import axios from 'axios';
import { useRouter } from 'vue-router';
import Toast from '@/components/Toast.vue';

const email = ref('');
const password = ref('');
const forgotEmail = ref('');
const isLoading = ref(false);
const isResetting = ref(false);
const showForgot = ref(false);
const isAdmin = ref(false); // toggle

const role = computed(() => (isAdmin.value ? 'admin' : 'member'));
const router = useRouter();
const apiBaseUrl = process.env.VUE_APP_API_URL;
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

const login = async () => {
  isLoading.value = true;
  try {
    const { data } = await axios.post(`${apiBaseUrl}/api/account/login`, {
      email: email.value,
      password: password.value,
      role: role.value
    });

    const { token, user } = data;
    localStorage.setItem('authToken', token);
    localStorage.setItem('userRole', user.role);
    localStorage.setItem('userId', user.id);

    showToast('Login successful!', 'success');

    setTimeout(() => {
      router.push(user.role === 'admin' ? '/artisan' : '/');
    }, 500);
  } catch (error) {
    const msg = error.response?.data?.message || error.response?.data?.error || 'Login failed.';
    showToast(msg, 'error');
  } finally {
    isLoading.value = false;
  }
};

const sendResetLink = async () => {
  if (!forgotEmail.value) return showToast('Please enter your email.', 'error');

  isResetting.value = true;
  try {
    const res = await axios.post(`${apiBaseUrl}/api/v1/password/forgot`, {
      email: forgotEmail.value,
    });
    showToast(res.data.message || 'Reset link sent. Check your inbox!', 'success');
    forgotEmail.value = '';
    showForgot.value = false;
  } catch (error) {
    const msg = error.response?.data?.message || error.response?.data?.error || 'Failed to send reset link.';
    showToast(msg, 'error');
  } finally {
    isResetting.value = false;
  }
};
</script>

<style scoped>
.loader {
  border: 3px solid #333;
  border-top: 3px solid #FFD700;
  border-radius: 50%;
  width: 18px;
  height: 18px;
  animation: spin 1s linear infinite;
  margin-right: 8px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
.animate-fade-in {
  animation: fadeIn 0.4s ease-out both;
}
.animate-slide-down {
  animation: slideDown 0.3s ease-out both;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
