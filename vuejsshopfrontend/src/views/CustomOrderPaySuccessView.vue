<template>
  <div class="success-container min-h-screen flex flex-col">
    <!-- Navbar -->
    <NavBar />

    <!-- Success Section -->
    <section class="success flex-grow py-10 px-4 sm:px-8 bg-gradient-to-b from-green-50 to-white">
      <div class="container mx-auto bg-white bg-opacity-80 p-6 rounded-lg shadow-xl w-full sm:w-4/5 md:w-3/5">

        <!-- Loading State -->
        <div v-if="isLoading" class="flex flex-col items-center justify-center py-10">
          <!-- Tailwind spinner -->
          <svg class="animate-spin h-8 w-8 text-green-600 mb-4" fill="none" viewBox="0 0 24 24">
            <circle
                class="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                stroke-width="4"
            />
            <path
                class="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8v8H4z"
            />
          </svg>
          <span class="text-green-700 font-semibold text-lg">Verifying Payment...</span>
        </div>

        <!-- Success Content -->
        <div v-else>
          <h1 class="text-3xl font-extrabold text-green-600 mb-4 text-center">
            Custom Order Payment Successful!
          </h1>
          <p class="text-gray-700 text-center mb-6">
            Thank you for your custom order. Your payment was processed successfully.
          </p>

          <!-- Payment Details Card -->
          <div v-if="!apiError" class="details bg-gray-50 p-6 rounded-lg shadow mb-4">
            <p class="mb-2">
              <strong class="font-semibold">Custom Order ID:</strong>
              <span class="ml-1">{{ customOrderId }}</span>
            </p>
            <p class="mb-2">
              <strong class="font-semibold">Amount Paid:</strong>
              <span class="ml-1">रु {{ amount }}</span>
            </p>
            <p>
              <strong class="font-semibold">Transaction ID:</strong>
              <span class="ml-1">{{ transactionId }}</span>
            </p>
          </div>

          <!-- Error Message -->
          <div v-if="apiError" class="text-red-600 text-center my-4 font-semibold">
            {{ apiError }}
          </div>

          <!-- Return to Custom Orders Button -->
          <router-link to="/custom-orders" class="btn-primary text-center block mt-6">
            Return to Custom Orders
          </router-link>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <Footer />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';
import { useRoute } from 'vue-router';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';

const route = useRoute();

// Reactive state for custom order details, errors, and loading status
const customOrderId = ref('');
const amount = ref('');
const transactionId = ref('');
const apiError = ref('');
const apiBaseUrl = process.env.VUE_APP_API_URL;
const isLoading = ref(true);

onMounted(async () => {
  try {
    // Retrieve the Base64-encoded response from the URL query parameter "data"
    const encodedResponse = route.query.data; // e.g., "eyJ0cmFuc2FjdGlvbl91dWlkIjoiMDAw..."
    if (!encodedResponse) {
      apiError.value = 'Response data is missing.';
      isLoading.value = false;
      return;
    }

    // Decode the response from Base64 and parse it as JSON
    const decodedResponse = atob(encodedResponse);
    const parsedResponse = JSON.parse(decodedResponse);

    // Extract the transaction_uuid from the parsed response
    const transactionIdFromResponse = parsedResponse.transaction_uuid;
    if (!transactionIdFromResponse) {
      apiError.value = 'Transaction ID is missing.';
      isLoading.value = false;
      return;
    }

    // Retrieve the user's token
    const token = localStorage.getItem('authToken');
    if (!token) {
      apiError.value = 'User is not authenticated. Please log in.';
      isLoading.value = false;
      return;
    }

    // Call the custom order payment verification endpoint
    const response = await axios.post(
        `${apiBaseUrl}/api/v1/custom-orders/payment/verify`,
        { transaction_id: transactionIdFromResponse },
        { headers: { Authorization: `Bearer ${token}` } }
    );

    // Update our UI state with the verified details from the API
    customOrderId.value = response.data.custom_order_id;
    amount.value = response.data.amount_paid;
    transactionId.value = response.data.transaction_id;
  } catch (error) {
    console.error('Error verifying payment:', error);
    apiError.value = error.response?.data?.message || 'Unable to verify payment. Please try again.';
  } finally {
    // Loading is complete, whether success or error
    isLoading.value = false;
  }
});
</script>

<style scoped>
.success-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
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
  color: #374151; /* Gray-700 */
  line-height: 1.6;
}

.btn-primary {
  display: inline-block;
  width: 100%;
  text-align: center;
  padding: 0.75rem;
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
