<template>
  <div class="min-h-screen flex flex-col bg-[#0f0f0f] text-[#C8B280] font-sans">
    <NavBar />

    <main class="flex flex-col lg:flex-row flex-grow min-h-[calc(100vh-4rem)]">
      <!-- Left Logo Section -->
      <div class="w-full lg:w-1/2 bg-[#1a1a1a] flex items-center justify-center px-6 py-6">
        <div class="text-center">
          <img src="@/assets/pustakalaya-logo.png" alt="Pustakalaya Logo" class="w-64 lg:w-72 mx-auto" />
          <h1 class="text-[#C8B280] text-4xl mt-4 font-semibold">Pustakalaya</h1>
        </div>
      </div>

      <!-- Right Auth Section -->
      <div class="w-full lg:w-1/2 flex flex-col items-center justify-start px-4 py-6">
        <!-- Toggle Buttons Only (no ghost wrapper) -->
        <div class="flex justify-center mb-6 space-x-0">
          <button
            @click="isLogin = true"
            :class="isLogin ? 'bg-[#C8B280] text-black' : 'bg-transparent text-[#C8B280] hover:bg-[#2c2c2c]'"
            class="px-6 py-2 font-semibold transition duration-300 rounded-l-full border border-[#C8B280]">
            Login
          </button>
          <button
            @click="isLogin = false"
            :class="!isLogin ? 'bg-[#C8B280] text-black' : 'bg-transparent text-[#C8B280] hover:bg-[#2c2c2c]'"
            class="px-6 py-2 font-semibold transition duration-300 rounded-r-full border border-[#C8B280]">
            Register
          </button>
        </div>

        <!-- Form Component -->
        <component
          :is="isLogin ? LoginForm : RegisterForm"
          class="w-full max-w-md  p-8 "
        />
      </div>
    </main>

    <FooterComponent />
  </div>
</template>


<script setup>
import { ref } from 'vue';
import NavBar from '@/components/NavBar.vue';
import FooterComponent from '@/components/Footer.vue';
import LoginForm from '@/components/LoginForm.vue';
import RegisterForm from '@/components/RegisterForm.vue';

const isLogin = ref(true);
</script>

<style scoped>
.font-sans {
  font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
}
</style>
