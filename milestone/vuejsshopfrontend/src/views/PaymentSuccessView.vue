<template>
  <div class="success-container min-h-screen flex flex-col bg-gray-100">
    <!-- Navbar -->
    <NavBar />

    <!-- Success Section -->
    <section class="success flex-grow bg-cover bg-center py-10 px-4 sm:px-8">
      <div class="container mx-auto bg-white bg-opacity-70 p-6 rounded-lg shadow-lg w-4/5 md:w-3/5">
        <!-- Loading State -->
        <div v-if="isLoading" class="flex flex-col items-center justify-center py-10">
          <svg class="animate-spin h-8 w-8 text-green-600 mb-4" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
          </svg>
          <span class="text-green-700 font-semibold text-lg">Verifying Payment...</span>
        </div>

        <!-- Success Content -->
        <div v-else>
          <h1 class="text-3xl font-bold text-green-600 mb-6 text-center">
            Payment Successful!
          </h1>
          <p class="text-gray-700 text-center mb-6">
            Thank you for your purchase. Your payment was processed successfully.
          </p>

          <!-- Payment Details Card -->
          <div v-if="!apiError" class="details bg-gray-50 p-6 rounded shadow mb-4">
            <p><strong>Order ID:</strong> {{ orderId }}</p>
            <p><strong>Amount Paid:</strong>रु {{ amount }}</p>
            <p><strong>Transaction ID:</strong> {{ transactionId }}</p>
          </div>

          <!-- Error Message -->
          <div v-if="apiError" class="text-red-600 text-center my-4 font-semibold">
            {{ apiError }}
          </div>

          <!-- Return to Shop Button -->
          <router-link to="/shop" class="btn-primary text-center block">
            Return to Shop
          </router-link>

          <!-- Auto-redirect Message -->
          <div v-if="!apiError" class="mt-4 text-center text-gray-700">
            You will be redirected in {{ redirectCountdown }} seconds...
          </div>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <Footer />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import axios from 'axios';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';

const route = useRoute();
const router = useRouter();

const orderId = ref('');
const amount = ref('');
const transactionId = ref('');
const apiError = ref('');
const isLoading = ref(true);
const redirectCountdown = ref(5);
const apiBaseUrl = process.env.VUE_APP_API_URL;

onMounted(async () => {
  try {
    // Get the Base64-encoded response from the URL query parameter "data"
    const encodedResponse = route.query.data; // e.g., "eyJ0cmFuc2FjdGlvbl91dWlkIjoiMDAw..."
    if (!encodedResponse) {
      apiError.value = 'Response data is missing.';
      isLoading.value = false;
      return;
    }

    // Decode the Base64-encoded response and parse as JSON
    const decodedResponse = atob(encodedResponse);
    const parsedResponse = JSON.parse(decodedResponse);

    // Extract transaction ID from the parsed response
    const transactionIdFromResponse = parsedResponse.transaction_uuid;
    if (!transactionIdFromResponse) {
      apiError.value = 'Transaction ID is missing.';
      isLoading.value = false;
      return;
    }

    // Retrieve user's token from localStorage
    const token = localStorage.getItem('authToken');
    if (!token) {
      apiError.value = 'User is not authenticated. Please log in.';
      isLoading.value = false;
      return;
    }

    // Call the backend API to verify payment details
    const response = await axios.post(
        `${apiBaseUrl}/api/v1/payment/verify`,
        { transaction_id: transactionIdFromResponse },
        { headers: { Authorization: `Bearer ${token}` } }
    );

    // Update UI state with the API response
    orderId.value = response.data.order_id;
    amount.value = response.data.amount_paid;
    transactionId.value = response.data.transaction_id;

    // Start redirect countdown (auto-redirect to shop after countdown)
    const countdownInterval = setInterval(() => {
      if (redirectCountdown.value > 0) {
        redirectCountdown.value--;
      } else {
        clearInterval(countdownInterval);
        router.push('/shop');
      }
    }, 1000);
  } catch (error) {
    console.error('Error verifying payment:', error);
    apiError.value = error.response?.data?.message || 'Unable to verify payment. Please try again.';
  } finally {
    isLoading.value = false;
  }
});
</script>

<style scoped>
.success-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #f9fafb;
  overflow-x: hidden;
}

.success {
  flex-grow: 1;
  background-size: cover;
  background-repeat: no-repeat;
  background-position: center;
  overflow-x: hidden;
}

.details {
  font-size: 1.1rem;
  color: #374151;
  line-height: 1.6;
}

.btn-primary {
  display: inline-block;
  width: 100%;
  text-align: center;
  padding: 0.75rem;
  margin-top: 1.5rem;
  background-color: #2563eb; /* Blue-600 */
  color: white;
  text-decoration: none;
  border-radius: 0.375rem;
  font-weight: 600;
  transition: background-color 0.3s ease, transform 0.2s ease;
}

.btn-primary:hover {
  background-color: #1e40af; /* Blue-800 */
  transform: translateY(-2px);
}
</style>
