<template>
  <div class="connect-container min-h-screen flex flex-col">
    <!-- NavBar -->
    <NavBar />

    <!-- Toast Notification -->
    <Toast :message="toastMessage" :type="toastType" :visible="toastVisible" />

    <main class="flex-grow">
      <section class="bg-gray-100 py-6">
        <div class="container mx-auto max-w-md bg-white p-6 rounded shadow-md">
          <h1 class="text-2xl font-bold mb-4 text-center">Reset Your Password</h1>
          <form @submit.prevent="submitReset">
            <div class="mb-4">
              <label class="block text-gray-700 mb-1">New Password</label>
              <input
                  type="password"
                  v-model="password"
                  placeholder="Enter new password"
                  class="w-full px-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-300"
              />
            </div>
            <div class="mb-4">
              <label class="block text-gray-700 mb-1">Confirm New Password</label>
              <input
                  type="password"
                  v-model="password_confirmation"
                  placeholder="Confirm new password"
                  class="w-full px-3 py-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-300"
              />
            </div>
            <div class="flex justify-center">
              <button
                  type="submit"
                  class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition disabled:opacity-50"
                  :disabled="!canSubmit"
              >
                Reset Password
              </button>
            </div>
          </form>
        </div>
      </section>
    </main>

    <!-- Footer -->
    <Footer />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import axios from 'axios';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';
import Toast from '@/components/Toast.vue';

const route = useRoute();
const router = useRouter();
const apiBaseUrl = process.env.VUE_APP_API_URL;

// Form fields and toast state
const token = ref('');
const email = ref('');
const password = ref('');
const password_confirmation = ref('');
const toastMessage = ref('');
const toastType = ref('success'); // 'success' or 'error'
const toastVisible = ref(false);

// Extract token and email from query parameters when the component mounts.
onMounted(() => {
  token.value = route.query.token || '';
  email.value = route.query.email || '';
  if (!token.value || !email.value) {
    showToast('Invalid password reset link.', 'error');
  }
});

// Computed property to enable the submit button only if both fields are not empty.
const canSubmit = computed(() => {
  return password.value.trim() !== '' && password_confirmation.value.trim() !== '';
});

// Helper to show Toast notifications.
const showToast = (message, type = 'success') => {
  toastMessage.value = message;
  toastType.value = type;
  toastVisible.value = true;
  setTimeout(() => {
    toastVisible.value = false;
  }, 3000);
};

const submitReset = async () => {
  // Check for non-matching passwords before sending.
  if (!canSubmit.value) {
    showToast('Please fill in both fields.', 'error');
    return;
  }
  if (password.value !== password_confirmation.value) {
    showToast('Passwords do not match.', 'error');
    return;
  }

  try {
    const res = await axios.post(`${apiBaseUrl}/api/v1/password/reset`, {
      token: token.value,
      email: email.value,
      password: password.value,
      password_confirmation: password_confirmation.value
    });

    // On success, show success message in toast.
    showToast(res.data.message || 'Password reset successfully. Redirecting to login...', 'success');
    setTimeout(() => {
      router.push('/auth');
    }, 2000);

  } catch (err) {
    console.error('Error resetting password:', err);

    // 1) Check if it's a validation error set: err.response?.data?.errors
    if (err.response?.data?.errors) {
      // For example, Laravel might return:
      // {
      //   "message": "The given data was invalid.",
      //   "errors": {
      //     "password": [
      //       "The password must be at least 8 characters..."
      //     ]
      //   }
      // }
      const errorsObject = err.response.data.errors;
      // Flatten arrays of validation messages
      const combinedMessages = Object.values(errorsObject)
          .flat()
          .join(' ');

      showToast(combinedMessages, 'error');

      // 2) If no 'errors' but a single 'message' key is provided
    } else if (err.response?.data?.message) {
      // e.g. "This password reset token is invalid."
      showToast(err.response.data.message, 'error');

      // 3) Otherwise, a fallback error
    } else {
      showToast('An error occurred while resetting the password.', 'error');
    }
  }
};


</script>

<style scoped>
.connect-container {
  /* Optional global container styling if needed */
}
</style>
